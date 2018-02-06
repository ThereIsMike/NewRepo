using Android.App;
using Android.Widget;
using Android.OS;
using System.Linq;
using Microsoft.WindowsAzure.MobileServices;
using Microsoft.WindowsAzure.MobileServices.SQLiteStore;
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
using System.Collections.ObjectModel;

namespace ATaskIt
{
    [Activity(Label = "ATaskIt", MainLauncher = true)]
    public class MainActivity : Activity
    {
        private ObservableCollection<Data.Status> _status_sync_enum;
        private ObservableCollection<Data.Status> _status_sync_enum_offline;
        private ImageButton addItem;
        private ProgressDialog busyIndicator;
        private TextView debuginformation;
        private ItemManager item_manager;
        private ListView Items;
        private MobileServiceClient MobileService;
        private ElementAdapter myTasks;
        private TextView newItem;
        private LinearLayout newItemField;
        private List<Item> tasksOnly = new List<Item>();

        public static Context Instance { get; private set; }

        public bool online { get; private set; }

        public async Task<bool> DeleteExecuted()
        {
            var item_enum_async = await this.item_manager.GetItemsAsync();
            var status_enum_async = await this.item_manager.GetStatusAsync();
            var status_enum_to_delete = status_enum_async.Where(s => s.Executed);

            foreach (var item in item_enum_async)
            {
                var itemstodelete = status_enum_to_delete.Where(i => i.Name == item.Name).FirstOrDefault();
                if (itemstodelete != null)
                {
                    await this.item_manager.DeleteItemAsync(item);
                    await this.item_manager.DeleteStatusAsync(itemstodelete);
                }
            }

            return true;
        }

        public async Task<bool> GetItemsAndDisplayAsync(bool with_sync)
        {
            this.MobileService = new MobileServiceClient(Settings.BASE_ADDRESS);
            this.busyIndicator = ProgressDialog.Show(this, "", "Updating...", true);
            this.busyIndicator.SetProgressStyle(ProgressDialogStyle.Spinner);

            try
            {
                var _item_sync_enum = await this.item_manager.GetItemsAsync(true);

                this.tasksOnly.Clear();
                if (_item_sync_enum != null)
                { // online
                    this.online = true;
                    this._status_sync_enum = await this.item_manager.GetStatusAsync(true);
                    this.debuginformation.Text = "Fresh from: " + DateTime.Now.ToString("dd-MM-yyyy HH:mm");
                    foreach (var item in _item_sync_enum)
                    {
                        this.tasksOnly.Add(item);
                    }
                }
                else
                { //offline
                    this.debuginformation.Text = "Backup from: " + DateTime.Now.ToString("dd-MM-yyyy HH:mm");
                    this.online = false;
                    var _item_sync_enum_offline = await this.item_manager.GetItemsAsync(false);
                    this._status_sync_enum_offline = await this.item_manager.GetStatusAsync(false);
                    foreach (var item in _item_sync_enum_offline)
                    {
                        this.tasksOnly.Add(item);
                    }
                }

                this.myTasks = new ElementAdapter(this, this.tasksOnly, this.item_manager, this.online ? this._status_sync_enum : this._status_sync_enum_offline);
            }
            catch (Exception ex)
            {
                this.debuginformation.Text = ex.Message;
                this.busyIndicator.Dismiss();
            }

            this.Items.Adapter = this.myTasks;
            this.busyIndicator.Dismiss();
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
            RunOnUiThread(async () => await GetItemsAndDisplayAsync(true));
            return base.OnNavigateUp();
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.TitleFormatted.ToString())
            {
                case "Refresh":
                    RunOnUiThread(async () => await GetItemsAndDisplayAsync(true));
                    return true;

                case "Delete":
                    Toast.MakeText(this, "Deleting... ",
                    ToastLength.Short).Show();
                    var deleting = Task.Run(async () => await this.DeleteExecuted());
                    deleting.ContinueWith(x => RunOnUiThread(async () => await GetItemsAndDisplayAsync(false)));
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

            this.item_manager = ItemManager.DefaultManager;

            var inputManager = (InputMethodManager)this.GetSystemService(Context.InputMethodService);

            this.newItemField = FindViewById<LinearLayout>(Resource.Id.NewItemField);
            this.newItem = FindViewById<TextView>(Resource.Id.NewItem);
            this.Items = FindViewById<ListView>(Resource.Id.ItemList);
            this.addItem = FindViewById<ImageButton>(Resource.Id.AddItem);
            this.debuginformation = FindViewById<TextView>(Resource.Id.debuginformation);

            //if (this.Intent.Extras != null)
            //{
            //    if (this.item_manager.IsOfflineEnabled)
            //        RunOnUiThread(async () => await GetItemsAndDisplayAsync(true));
            //}
            this.newItemField.Visibility = Android.Views.ViewStates.Gone;

            this.addItem.Click += async (o, e) =>
            {
                if (this.newItem.Text != "")
                {
                    await this.item_manager.SaveItemAsync(new Item { Name = this.newItem.Text });
                    this.newItem.Text = "";
                    RunOnUiThread(async () => await GetItemsAndDisplayAsync(false));
                }

                inputManager.HideSoftInputFromWindow(this.newItem.WindowToken, 0);
            };

            RunOnUiThread(async () => await GetItemsAndDisplayAsync(true));
        }

        private ProgressDialog GetBusyIndicator() => this.busyIndicator;
    }
}