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

namespace ATaskIt
{
    [Activity(Label = "ATaskIt", MainLauncher = true)]
    public class MainActivity : Activity
    {
        private Button addItem;
        private Button getItemList;
        private ListView Items;
        private Button logTokenButton;
        private MobileServiceClient MobileService;
        private TextView msgText;
        private TaskElementAdapter myTasks;
        private TextView newItem;
        private LinearLayout newItemField;
        private Button refreshList;
        private TextView serviceName;
        private List<Item> tasksOnly = new List<Item>();

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

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            InputMethodManager inputManager = (InputMethodManager)this.GetSystemService(Context.InputMethodService);

            this.newItemField = FindViewById<LinearLayout>(Resource.Id.NewItemField);
            this.serviceName = FindViewById<TextView>(Resource.Id.ServiceName);
            this.newItem = FindViewById<TextView>(Resource.Id.NewItem);
            this.Items = FindViewById<ListView>(Resource.Id.ItemList);
            this.refreshList = FindViewById<Button>(Resource.Id.RefreshList);
            this.getItemList = FindViewById<Button>(Resource.Id.GetItemList);
            this.addItem = FindViewById<Button>(Resource.Id.AddItem);
            this.msgText = FindViewById<TextView>(Resource.Id.msgText);
            this.logTokenButton = FindViewById<Button>(Resource.Id.logTokenButton);

            if (this.Intent.Extras != null)
            {
                foreach (var key in this.Intent.Extras.KeySet())
                {
                    var value = this.Intent.Extras.GetString(key);
                    Log.Debug("MainActivity", "Key: {0} Value: {1}", key, value);
                }
            }
            IsPlayServicesAvailable();

            this.logTokenButton.Click += (o, e) =>
            {
                var token = FirebaseInstanceId.Instance.Token;
                Log.Debug("MainAvtivity", "InstanceID token: " + token);
            };

            this.newItemField.Visibility = Android.Views.ViewStates.Gone;

            this.refreshList.Click += (o, e) =>
            {
                this.MobileService = new MobileServiceClient(
                $"https://{this.serviceName.Text}.azurewebsites.net");
                inputManager.HideSoftInputFromWindow(this.refreshList.WindowToken, 0);
                this.serviceName.Visibility = Android.Views.ViewStates.Gone;
                this.refreshList.Visibility = Android.Views.ViewStates.Gone;
            };

            this.getItemList.Click += async (o, e) =>
            {
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
                IMobileServiceTable<Item> _item = this.MobileService.GetTable<Item>();
                await _item.InsertAsync(new Item { Name = this.newItem.Text });
                inputManager.HideSoftInputFromWindow(this.refreshList.WindowToken, 0);
            };
        }
    }
}