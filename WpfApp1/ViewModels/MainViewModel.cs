using Microsoft.WindowsAzure.MobileServices;
using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Data.SqlClient;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace WpfApp1
{
    public class MainViewModel : IViewModelBase
    {
        public IReactiveProperty<int> NumberofItems { get; set; } = new ReactiveProperty<int>();

        public IReactiveProperty<int> NumberofUsers { get; set; } = new ReactiveProperty<int>();
        public ObservableCollection<Item> List { get; set; } = new ObservableCollection<Item>();

        public ObservableCollection<Status> ListShow { get; set; } = new TrulyObservableCollection<Status>();
        public Collection<string> ListShowDb { get; set; } = new Collection<string>();

        public ObservableCollection<User> UserList { get; set; } = new ObservableCollection<User>();

        public ReactiveProperty<bool> PushProduct { get; set; } = new ReactiveProperty<bool>();

        public ReactiveProperty<bool> ProductUpdated { get; set; } = new ReactiveProperty<bool>();

        public ReactiveProperty<string> ProductName { get; set; } = new ReactiveProperty<string>("Product");

        public bool isLoaded { get; set; }


         public MainViewModel()
        {
            
            Subscriptions();
            UpdateLists();
            this.isLoaded = true;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        void Subscriptions()
        {      
            // Pushing new products to the product db
            this.PushProduct.Subscribe(async x =>
           {
               if (x)
               {
                   //Just Showing product name
                   Console.WriteLine(this.ProductName.Value.ToString());
                   await App.MobileService.GetTable<Item>().InsertAsync(new Item() { Name = this.ProductName.Value.ToString() });
                   this.ProductName.Value = "";
                   this.ProductUpdated.Value = true;
               }
           });
            // If slader moved to add new product.
            Observable.Interval(TimeSpan.FromSeconds(0.5)).ObserveOnDispatcher().Subscribe(_ =>
            {
                if (this.ProductUpdated.Value)
                {
                    this.PushProduct.Value = !this.PushProduct.Value;
                    this.ProductUpdated.Value = false;
                    UpdateLists();
                }
            });
            // Adding event to the handler, so we can react on collection change
            this.ListShow.CollectionChanged += this.MyItemsSource_CollectionChangedAsync;
        }

        async void MyItemsSource_CollectionChangedAsync(object sender, NotifyCollectionChangedEventArgs e)
        {
            var x = sender as TrulyObservableCollection<Status>;
            if (x != null && this.ListShowDb.Count == x.Count)
                // i is the index
                foreach (var item in x.Select((value, i) => new { i, value }))
                {
                    // Checking is selection has been made for this products
                    if (item.value.Selected != this.ListShowDb[item.i])
                    {
                        // This excuted when selection of a buyer has been made
                        Console.WriteLine($"Assigned  { item.value.Selected} to buy { item.value.Name}");
                        // Updating the element in the list
                        this.ListShowDb[item.i] = item.value.Selected;

                        IMobileServiceTable<Status> _status = App.MobileService.GetTable<Status>();
                        MobileServiceCollection<Status, Status> _statusenum = await _status
                                .Where(u => u.Name != string.Empty)
                                .ToCollectionAsync();
                        if (_statusenum.Any(p => p.Name == item.value.Name))
                        {
                            // Updating the db with the buyer name selected information
                           var modified =  _statusenum.SingleOrDefault(p => p.Name == item.value.Name);
                            var cc = await _status.LookupAsync(modified.Id);
                            await _status.DeleteAsync(cc);
                            cc.Selected = item.value.Selected;
                            await _status.InsertAsync(cc);
                        }
                        else
                        {
                            // Adding information into db with the buyer name selected
                            await App.MobileService.GetTable<Status>().InsertAsync(new Status() { Name = item.value.Name, Selected = item.value.Selected, CreatedAt = null });
                        }
                    }
                    else
                    {
                        // this is executed when shopping was done
                        IMobileServiceTable<Status> _status = App.MobileService.GetTable<Status>();
                        MobileServiceCollection<Status, Status> _statusenum = await _status
                                .Where(u => u.Name != string.Empty)
                                .ToCollectionAsync();
                        var excuted = _statusenum.SingleOrDefault(g => g.Name == item.value.Name);

                            if (excuted != null && item.value.Executed != excuted.Executed)
                            {
                            // Changing information about execution
                            Console.WriteLine($" {item.value.Name}  was bought = {item.value.Executed}");
                            var modified = _statusenum.SingleOrDefault(g => g.Name == item.value.Name);
                            await App.MobileService.GetTable<Status>().DeleteAsync(modified);
                            modified.Executed = item.value.Executed;
                            await App.MobileService.GetTable<Status>().InsertAsync(modified);
                        }
                    }
                }
            if (x.Count != this.ListShowDb.Count)
            {
                this.ListShowDb.Clear();
                foreach (var item in x)
                {
                    this.ListShowDb.Add(item.Selected);
                }
            }
        }
        async void UpdateLists()
        {
            IMobileServiceTable<Item> _products = App.MobileService.GetTable<Item>();
            MobileServiceCollection<Item, Item> _productsenum = await _products
                    .Where(i => i.Name != string.Empty)
                    .ToCollectionAsync();
            foreach (var item in _productsenum)
                {
                    // Adding products to the list from the db
                    if (!this.List.Any(x => x.Name == item.Name))
                        this.List.Add(item);
                }
                // Gettings number of products
                this.NumberofItems.Value = this.List.Count();

            
            IMobileServiceTable<User> _users = App.MobileService.GetTable<User>();
            MobileServiceCollection<User, User> _usersenum = await _users
                    .Where(u => u.FirstName != string.Empty)
                    .ToCollectionAsync();
            foreach (var user in _usersenum)
            {
                    if (!this.UserList.Any(x => x.Id == user.Id))
                    {
                        // Adding users to the list from the db
                        this.UserList.Add(user);
                    }
             }
                // Gettings number of buyers
                this.NumberofUsers.Value = this.UserList.Count();

                foreach (var item in this.List)
                {
                    var Li = new ObservableCollection<User>();
                    foreach (var us in this.UserList)
                    {
                        // Gettings Buyers list from the db 
                        Li.Add(us);
                    }
                    if (!this.ListShow.Any(x => x.Name == item.Name))
                    {
                    IMobileServiceTable<Status> _status = App.MobileService.GetTable<Status>();
                    MobileServiceCollection<Status, Status> _statusenum = await _status
                            .Where(u => u.Name != string.Empty)
                            .ToCollectionAsync();
                    if (_statusenum.Any(p => p.Name == item.Name))
                        {
                            // Get selection from the db and show
                            var selecteduser = _statusenum.SingleOrDefault(p => p.Name == item.Name).Selected;
                            this.ListShow.Add(new Status(new ReactiveProperty<User>(this.UserList.SingleOrDefault(g => g.FirstName == selecteduser)), new ReactiveProperty<bool>(_statusenum.SingleOrDefault(p => p.Name == item.Name).Executed)) { Name = item.Name, UserList = Li });
                        }
                        else
                        {
                            // Get selection from the db and show
                            this.ListShow.Add(new Status { Name = item.Name, UserList = Li });
                        }
                    }
                }
        }

        public void RaisePropertyChangedEvent(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChangedEventArgs e = new PropertyChangedEventArgs(propertyName);
                PropertyChanged(this, e);
            }
        }
    }
}
