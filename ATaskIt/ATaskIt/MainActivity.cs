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

namespace ATaskIt
{
    [Activity(Label = "ATaskIt", MainLauncher = true)]
    public class MainActivity : Activity
    {
        private LinearLayout newItemField;
        private TextView serviceName;
        private TextView newItem;
        private ListView Items;
        private Button refreshList;
        private Button getItemList;
        private Button addItem;
        private MobileServiceClient MobileService;
        private TaskElementAdapter myTasks;
        private List<Item> tasksOnly = new List<Item>();
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

            this.newItemField.Visibility = Android.Views.ViewStates.Gone;

            this.refreshList.Click += (o, e) => { this.MobileService = new MobileServiceClient(
                $"https://{this.serviceName.Text}.azurewebsites.net");
                inputManager.HideSoftInputFromWindow(this.refreshList.WindowToken, 0);
                this.serviceName.Visibility = Android.Views.ViewStates.Gone;
                this.refreshList.Visibility = Android.Views.ViewStates.Gone;
            };

            this.getItemList.Click += async (o, e) => {
                IMobileServiceTable<Item> _item = this.MobileService.GetTable<Item>();
                MobileServiceCollection<Item, Item> _productsenum = await _item
                        .Where(u => u.Name != string.Empty)
                        .ToCollectionAsync();
                this.tasksOnly.Clear();
                foreach (var item in _productsenum)
                {
                    this.tasksOnly.Add(item);
                }
                this.myTasks = new TaskElementAdapter(this, this.tasksOnly);
                this.Items.Adapter = this.myTasks;
                this.newItemField.Visibility = Android.Views.ViewStates.Visible;              
            };

             this.addItem.Click += async (o, e) => {
                 IMobileServiceTable<Item> _item = this.MobileService.GetTable<Item>();
                 await _item.InsertAsync(new Item { Name = this.newItem.Text });
                 inputManager.HideSoftInputFromWindow(this.refreshList.WindowToken, 0);
            };
        }
        
    }
}

