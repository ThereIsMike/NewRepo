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
        private TextView serviceName;
        private ListView Items;
        private Button refreshList;
        private Button getItemList;
        private MobileServiceClient MobileService;
        private TaskElementAdapter myTasks;
        private List<Item> tasksOnly = new List<Item>();
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            InputMethodManager inputManager = (InputMethodManager)this.GetSystemService(Context.InputMethodService);

            

            this.serviceName = FindViewById<TextView>(Resource.Id.ServiceName);
            this.Items = FindViewById<ListView>(Resource.Id.ItemList);
            this.refreshList = FindViewById<Button>(Resource.Id.RefreshList);
            this.getItemList = FindViewById<Button>(Resource.Id.GetItemList);
            this.refreshList.Click += (o, e) => { this.MobileService = new MobileServiceClient(
                $"https://{this.serviceName.Text}.azurewebsites.net");
                inputManager.HideSoftInputFromWindow(this.refreshList.WindowToken, 0);
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
                //this.myTasks = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleListItem1, this.namesOnly);
                this.myTasks = new TaskElementAdapter(this, this.tasksOnly);
                this.Items.Adapter = this.myTasks;


            };
        }
        
    }
}

