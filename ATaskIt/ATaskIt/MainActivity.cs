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
using Android.Views;

namespace ATaskIt
{
    [Activity(Label = "ATaskIt", MainLauncher = true)]
    public class MainActivity : Activity
    {
        private ImageButton addItem;
        private ListView Items;
        private MobileServiceClient MobileService;
        private TaskElementAdapter myTasks;
        private TextView newItem;
        private LinearLayout newItemField;
        private List<Item> tasksOnly = new List<Item>();

        public static Context Instance { get; private set; }

        public async Task<bool> DeleteExecuted()
        {
            var _item = this.MobileService.GetTable<Item>();
            var _status = this.MobileService.GetTable<Data.Status>();

            var _statussenum = await _status
                   .Where(u => u.Executed)
                   .ToCollectionAsync();

            var _productsenum = await _item
            .Where(u => u.Name != string.Empty)
            .ToCollectionAsync();

            foreach (var item in _productsenum)
            {
                var itemstodelete = _statussenum.Where(i => i.Name == item.Name).FirstOrDefault();
                if (itemstodelete != null)
                {
                    await _item.DeleteAsync(item);
                    await _status.DeleteAsync(itemstodelete);
                }
            }

            return true;
        }

        public async Task<bool> GetItemsAndDisplayAsync()
        {
            this.MobileService = new MobileServiceClient(Settings.BASE_ADDRESS);
            var _item = this.MobileService.GetTable<Item>();
            var _status = this.MobileService.GetTable<Data.Status>();

            var _productsenum = await _item
                    .Where(u => u.Name != string.Empty)
                    .ToCollectionAsync();
            var _statussenum = await _status
                   .Where(u => u.Name != string.Empty)
                   .ToCollectionAsync();
            this.tasksOnly.Clear();
            foreach (var item in _productsenum)
            {
                this.tasksOnly.Add(item);
            }
            this.myTasks = new TaskElementAdapter(this, this.tasksOnly, _item, _status, _statussenum);

            this.Items.Adapter = this.myTasks;
            this.newItemField.Visibility = Android.Views.ViewStates.Visible;
            return true;
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            this.MenuInflater.Inflate(Resource.Menu.top_menus, menu);
            return base.OnCreateOptionsMenu(menu);
        }

        public override bool OnNavigateUp()
        {
            RunOnUiThread(async () => await GetItemsAndDisplayAsync());
            return base.OnNavigateUp();
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.TitleFormatted.ToString())
            {
                case "Refresh":
                    RunOnUiThread(async () => await GetItemsAndDisplayAsync());
                    return true;

                case "Delete":
                    Toast.MakeText(this, "Deleting... ",
                    ToastLength.Short).Show();
                    var deleting = Task.Run(async () => await this.DeleteExecuted());
                    deleting.ContinueWith(x => RunOnUiThread(async () => await GetItemsAndDisplayAsync()));
                    return true;

                case "Preferences":
                    Toast.MakeText(this, "Action selected: " + item.TitleFormatted,
                    ToastLength.Short).Show();
                    return true;

                case "Register":
                    Toast.MakeText(this, "Registering...", ToastLength.Short).Show();
                    Task.Run(async () =>
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
                    return true;

                case "Unregister":
                    Toast.MakeText(this, "Unregistering...", ToastLength.Short).Show();
                    Task.Run(async () =>
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
                    return true;

                default:
                    break;
            }
            return base.OnOptionsItemSelected(item);
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Instance = this;
            SetContentView(Resource.Layout.Main);
            var toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
            SetActionBar(toolbar);
            this.ActionBar.Title = "Task It";

            var inputManager = (InputMethodManager)this.GetSystemService(Context.InputMethodService);

            this.newItemField = FindViewById<LinearLayout>(Resource.Id.NewItemField);
            this.newItem = FindViewById<TextView>(Resource.Id.NewItem);
            this.Items = FindViewById<ListView>(Resource.Id.ItemList);
            this.addItem = FindViewById<ImageButton>(Resource.Id.AddItem);

            if (this.Intent.Extras != null)
            {
                RunOnUiThread(async () => await GetItemsAndDisplayAsync());
            }
            this.newItemField.Visibility = Android.Views.ViewStates.Gone;

            this.addItem.Click += async (o, e) =>
            {
                if (this.newItem.Text != "")
                {
                    this.MobileService = new MobileServiceClient(Settings.BASE_ADDRESS);
                    var _item = this.MobileService.GetTable<Item>();
                    await _item.InsertAsync(new Item { Name = this.newItem.Text });
                    this.newItem.Text = "";
                }

                inputManager.HideSoftInputFromWindow(this.newItem.WindowToken, 0);
            };

            RunOnUiThread(async () => await GetItemsAndDisplayAsync());
        }
    }
}