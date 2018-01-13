using System;
using Microsoft.Azure.NotificationHubs;
using System.Configuration;
using Microsoft.Azure.Mobile.Server;
using Microsoft.Azure.Mobile.Server.Config;
using Newtonsoft.Json;

namespace taskitnowService.Helpers
{
    public static class Notifications
    {
        private static NotificationHubClient client;

        public static NotificationHubClient GetHub(MobileAppSettingsDictionary settings)
        {
            if (client == null)
            {
                // Get the Notification Hubs credentials.
                try
                {
                    string notificationHubName = settings.NotificationHubName;
                    string notificationHubConnection = settings
                        .Connections[MobileAppSettingsKeys.NotificationHubConnectionString].ConnectionString;

                    // Create a new Notification Hub client.
                    client = NotificationHubClient.CreateClientFromConnectionString(notificationHubConnection, notificationHubName);

                    System.Diagnostics.Debug.WriteLine("Hub Name: " + notificationHubName + "Hub Connection" + notificationHubConnection);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("Upsss:" + ex.Message);
                }
            }

            return client;
        }
    }

    public class PushTemplate
    {
        [JsonProperty(PropertyName = "body")]
        public string Body { get; set; }
    }
}