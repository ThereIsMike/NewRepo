using Android.App;
using Android.Widget;
using Android.OS;
using System.Linq;
using Microsoft.WindowsAzure.MobileServices;
using static Android.Net.Wifi.WifiConfiguration;
using ATaskIt.Data;
using Android.Views.InputMethods;
using Android.Content;
using System.Collections.Generic;
using Android.Gms.Common;
using System;
using Firebase.Messaging;
using Firebase.Iid;
using Android.Util;
using System.Threading.Tasks;
using ToShare;

namespace ATaskIt
{
    [Activity(Label = "ATaskIt", MainLauncher = true)]
    public class MainActivity : Activity
    {
        private Button addItem;
        private Button deregister;
        private Button getItemList;
        private ListView Items;
        private Button logTokenButton;
        private MobileServiceClient MobileService;
        private TextView msgText;
        private TaskElementAdapter myTasks;
        private TextView newItem;
        private LinearLayout newItemField;
        private Button register;
        private Button subscribeButton;
        private List<Item> tasksOnly = new List<Item>();

        public static Context Instance { get; private set; }

        public bool IsPlayServicesAvailable()
        {
            int resultCode = GoogleApiAvailability.Instance.IsGooglePlayServicesAvailable(this);
            if (resultCode != ConnectionResult.Success)
            {
                if (GoogleApiAvailability.Instance.IsUserResolvableError(resultCode))
                    this.msgText.Text = GoogleApiAvailability.Instance.GetErrorString(resultCode);
                else
                {
                    this.msgText.Text = "This device is not supported";
                    Finish();
                }
                return false;
            }
            else
            {
                this.msgText.Text = "Google Play Services is available.";
                return true;
            }
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Instance = this;

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            InputMethodManager inputManager = (InputMethodManager)this.GetSystemService(Context.InputMethodService);

            this.newItemField = FindViewById<LinearLayout>(Resource.Id.NewItemField);
            this.newItem = FindViewById<TextView>(Resource.Id.NewItem);
            this.Items = FindViewById<ListView>(Resource.Id.ItemList);
            this.getItemList = FindViewById<Button>(Resource.Id.GetItemList);
            this.addItem = FindViewById<Button>(Resource.Id.AddItem);
            this.msgText = FindViewById<TextView>(Resource.Id.msgText);
            this.register = FindViewById<Button>(Resource.Id.register);
            this.deregister = FindViewById<Button>(Resource.Id.deregister);

            if (this.Intent.Extras != null)
            {
                foreach (var key in this.Intent.Extras.KeySet())
                {
                    var value = this.Intent.Extras.GetString(key);
                    Log.Debug("MainActivity", "Key: {0} Value: {1}", key, value);
                }
            }
            IsPlayServicesAvailable();

            this.newItemField.Visibility = Android.Views.ViewStates.Gone;

            this.getItemList.Click += async (o, e) =>
            {
                this.MobileService = new MobileServiceClient(Settings.BASE_ADDRESS);
                IMobileServiceTable<Item> _item = this.MobileService.GetTable<Item>();
                MobileServiceCollection<Item, Item> _productsenum = await _item
                        .Where(u => u.Name != string.Empty)
                        .ToCollectionAsync();
                this.tasksOnly.Clear();
                foreach (var item in _productsenum)
                {
                    this.tasksOnly.Add(item);
                }
                this.myTasks = new TaskElementAdapter(this, this.tasksOnly, _item);
                this.Items.Adapter = this.myTasks;
                this.newItemField.Visibility = Android.Views.ViewStates.Visible;
            };

            this.addItem.Click += async (o, e) =>
            {
                if (this.newItem.Text != "")
                {
                    this.MobileService = new MobileServiceClient(Settings.BASE_ADDRESS);
                    IMobileServiceTable<Item> _item = this.MobileService.GetTable<Item>();
                    await _item.InsertAsync(new Item { Name = this.newItem.Text });
                    this.newItem.Text = "";
                }

                inputManager.HideSoftInputFromWindow(this.newItem.WindowToken, 0);
            };
            this.register.Click += async (o, e) =>
            {
                await Task.Run(async () =>
                 {
                     try
                     {
                         var result = await NotificationRegistrationService.Instance.RegisterDeviceAsync();
                         if (!result)
                         {
                             System.Diagnostics.Debug.WriteLine("Error registering with notification hub");
                         }
                     }
                     catch (System.Exception ex)
                     {
                         System.Diagnostics.Debug.WriteLine($"[PushNotificationError]: Device registration failed with error {ex.Message}");
                     }
                 });
            };
            this.deregister.Click += async (o, e) =>
            {
                await Task.Run(async () =>
                 {
                     try
                     {
                         await NotificationRegistrationService.Instance.DeregisterDeviceAsync();
                         System.Diagnostics.Debug.WriteLine("Should be deregistered");
                     }
                     catch (System.Exception ex)
                     {
                         System.Diagnostics.Debug.WriteLine($"[PushNotificationError]: Device deregistration failed with error {ex.Message}");
                     }
                 });
            };
        }
    }
}