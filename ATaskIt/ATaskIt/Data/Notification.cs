using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Media;
using Android.OS;
using Android.Preferences;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using Firebase.Iid;
using Firebase.Messaging;
using Newtonsoft.Json;
using Polly;
using ToShare;

namespace ATaskIt.Data
{
    public interface IPushNotificationService
    {
        string GetDeviceId();

        DeviceInstallation GetDeviceRegistration(params string[] tags);
    }

    public abstract class BaseService
    {
        protected HttpClient client;

        protected BaseService(Action<HttpClient> httpClientModifier = null)
        {
            this.client = new HttpClient();
            httpClientModifier?.Invoke(this.client as HttpClient);
        }

        internal enum RequestType
        {
            Delete,
            Get,
            Post,
            Put
        }

        protected virtual string BaseAddress => string.Empty;

        protected Task<T> DeleteAsync<T>(string requestUri)
        {
            return SendWithRetryAsync<T>(RequestType.Delete, requestUri);
        }

        protected Task<T> PutAsync<T, K>(string requestUri, K obj) // where object
        {
            var jsonRequest = !obj.Equals(default(K)) ? JsonConvert.SerializeObject(obj) : null;
            return SendWithRetryAsync<T>(RequestType.Put, requestUri, jsonRequest);
        }

        private async Task<T> SendAsync<T>(RequestType requestType, string requestUri, string jsonRequest = null)
        {
            T result = default(T);

            HttpContent content = null;

            if (jsonRequest != null)
                content = new StringContent(jsonRequest, System.Text.Encoding.UTF8, "application/json");

            if (this.client.BaseAddress == null)
                this.client.BaseAddress = new Uri(this.BaseAddress);

            Task<HttpResponseMessage> httpTask;

            switch (requestType)
            {
                case RequestType.Delete:
                    httpTask = this.client.DeleteAsync(requestUri);
                    break;

                case RequestType.Put:
                    httpTask = this.client.PutAsync(requestUri, content);
                    break;

                default:
                    throw new Exception("Not a valid request type");
            }

            var response = await httpTask.ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
                return result;

            string json = string.Empty;

            if (response != null)
                json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            if (!string.IsNullOrEmpty(json))
                result = JsonConvert.DeserializeObject<T>(json);

            return result;
        }

        private async Task<T> SendWithRetryAsync<T>(RequestType requestType, string requestUri, string jsonRequest = null)
        {
            T result = default(T);

            result = await Policy
                .Handle<WebException>()
                .WaitAndRetryAsync(5, retryAttempt =>
                                   TimeSpan.FromMilliseconds((200 * retryAttempt)),
                    (exception, timeSpan, context) =>
                    {
                        Log.Debug("Sendwith Retry Async", exception.ToString());
                    }
                )
                .ExecuteAsync(async () => { return await SendAsync<T>(requestType, requestUri, jsonRequest); });

            return result;
        }
    }

    [Service]
    [IntentFilter(new[] { "com.google.firebase.INSTANCE_ID_EVENT" })]
    public class MyFirebaseIIDService : FirebaseInstanceIdService
    {
        private const string TAG = "MyFirebaseIIDService";

        public override void OnTokenRefresh()
        {
            // Called by Firebase when token is refreshed
            var refreshedToken = FirebaseInstanceId.Instance.Token;
            Log.Debug(TAG, "Refreshed token: " + refreshedToken);
            SendRegistrationToServer(refreshedToken);
        }

        private void SendRegistrationToServer(string token)
        {
            // Update registration to notification hub with updated token
            Task.Run(async () =>
            {
                await NotificationRegistrationService.Instance.RegisterDeviceAsync();
            });
        }
    }

    [Service]
    [IntentFilter(new[] { "com.google.firebase.MESSAGING_EVENT" })]
    public class NHubFirebaseMessagingService : FirebaseMessagingService
    {
        private const string TAG = "NHubFirebaseMsgService";

        public override void OnMessageReceived(RemoteMessage message)
        {
            Log.Debug(TAG, $"From: {message.From}");

            // Check if notification should be silent
            if (message.Data.ContainsKey("silent"))
            {
                var action = message.Data["action"];
                Log.Debug(TAG, $"Notification Message Action: {action}");
                PerformSilentNotification(action);
            }
            else
            {
                // Pull message body out of the template we registered with
                var messageBody = message.Data["message"];
                if (string.IsNullOrWhiteSpace(messageBody))
                    return;

                Log.Debug(TAG, $"Notification Message Body: {messageBody}");
                SendNotification(messageBody);
            }
        }

        private void PerformSilentNotification(string action)
        {
            System.Diagnostics.Debug.WriteLine($"[PNS] Perform action of type: {action}");
        }

        private void SendNotification(string messageBody)
        {
            // Display notification however necessary
            var intent = new Intent(this, typeof(MainActivity));
            intent.AddFlags(ActivityFlags.ClearTop);
            var pendingIntent = PendingIntent.GetActivity(this, 0, intent, PendingIntentFlags.OneShot);

            var notificationBuilder = new Notification.Builder(this)
                .SetSmallIcon(Resource.Drawable.icon)
                .SetSound(RingtoneManager.GetDefaultUri(RingtoneType.Notification))
                .SetContentTitle("Task:")
                .SetContentText(messageBody)
                .SetAutoCancel(true)
                .SetContentIntent(pendingIntent);

            var notificationManager = NotificationManager.FromContext(this);
            notificationManager.Notify(0, notificationBuilder.Build());
        }
    }

    public class NotificationRegistrationService : BaseService
    {
        private static NotificationRegistrationService instance;

        private NotificationRegistrationService()
            : base((HttpClient client) => client.DefaultRequestHeaders.Add("ZUMO-API-VERSION", "2.0.0"))
        { }

        public static NotificationRegistrationService Instance => instance ?? (instance = new NotificationRegistrationService());

        // TODO: Update with base address of server
        protected override string BaseAddress => Settings.BASE_ADDRESS;

        public Task DeregisterDeviceAsync()
        {
            var pushNotificationService = new PushNotificationService();

            // Get device installationid for notification hub
            var deviceId = pushNotificationService.GetDeviceId();

            if (deviceId == null)
                return Task.FromResult(false);

            // Delete that installation id from our NH
            return DeleteAsync<object>($"api/register/{deviceId}");
        }

        public Task<bool> RegisterDeviceAsync(params string[] tags)
        {
            // Resolve dep with whatever IOC container
            var pushNotificationService = new PushNotificationService();

            // Get our registration information
            var deviceInstallation = pushNotificationService?.GetDeviceRegistration(tags);

            if (deviceInstallation == null)
                return Task.FromResult(true);

            // Put the device information to the server
            return PutAsync<bool, DeviceInstallation>("api/register", deviceInstallation);
        }
    }

    public class PushNotificationService : IPushNotificationService
    {
        private const string INSTALLATION_ID_KEY = "nhub_install_id";

        public string GetDeviceId()
        {
            return this.GetInstallationId();
        }

        public DeviceInstallation GetDeviceRegistration(params string[] tags)
        {
            var installationId = this.GetInstallationId();

            if (FirebaseInstanceId.Instance.Token == null)
                return null;

            var installation = new DeviceInstallation
            {
                InstallationId = installationId,
                Platform = "gcm",
                PushChannel = FirebaseInstanceId.Instance.Token
            };

            // Set up tags to request
            installation.Tags.AddRange(tags);

            // Set up templates to request
            PushTemplate genericTemplate = new PushTemplate
            {
                Body = @"{""data"":{""message"":""$(messageParam)""}}"
            };
            PushTemplate silentTemplate = new PushTemplate
            {
                Body = @"{""data"":{""message"":""$(silentMessageParam)"", ""action"":""$(actionParam)"", ""silent"":""true""}}"
            };
            installation.Templates.Add("genericTemplate", genericTemplate);
            installation.Templates.Add("silentTemplate", silentTemplate);

            return installation;
        }

        private string GetInstallationId()
        {
            var installationId = string.Empty;

            // Use shared preferences to store Notification Hub InstallationId for reuse between app sessions
            var prefs = PreferenceManager.GetDefaultSharedPreferences(MainActivity.Instance);
            installationId = prefs.GetString(INSTALLATION_ID_KEY, string.Empty);

            if (string.IsNullOrWhiteSpace(installationId))
            {
                installationId = Guid.NewGuid().ToString();

                var editor = prefs.Edit();
                editor.PutString(INSTALLATION_ID_KEY, installationId);
                editor.Apply();

                // If we change the installation id, we need to invalidate the current Firebase
                // Instance Token so that we can deregister from the current
                // installationid/firebasetoken combination This will force a refresh of the Firebase
                // instance token. When the token is available, The FirebaseIIDService will process
                // the refresh with NH
                FirebaseInstanceId.Instance.DeleteInstanceId();
            }

            return installationId;
        }
    }
}