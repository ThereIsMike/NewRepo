using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp1
{
    public class MainViewModel : ViewModelBase
    {
        public IReactiveProperty<int> NumberofItems { get; set; } = new ReactiveProperty<int>();

        public IReactiveProperty<int> NumberofUsers { get; set; } = new ReactiveProperty<int>();
        public ObservableCollection<Products> List { get; set; } = new ObservableCollection<Products>();

        public ObservableCollection<ProductsShow> ListShow { get; set; } = new TrulyObservableCollection<ProductsShow>();
        public Collection<string> ListShowDb { get; set; } = new Collection<string>();

        public ObservableCollection<Buyers> UserList { get; set; } = new ObservableCollection<Buyers>();

        public ReactiveProperty<bool> PushProduct { get; set; } = new ReactiveProperty<bool>();

        public ReactiveProperty<bool> ProductUpdated { get; set; } = new ReactiveProperty<bool>();

        public ReactiveProperty<string> ProductName { get; set; } = new ReactiveProperty<string>("Product");


        public MainViewModel()
        {
            Subscriptions();
            UpdateLists();          
        }

        void Subscriptions()
        {      
            // Pushing new products to the product db
            this.PushProduct.Subscribe(x => 
            {
                if (x)
                {
                    //Just Showing product name
                    Console.WriteLine(this.ProductName.Value.ToString());
                    using (var db = new ShoppingContext())
                    {
                        db.ProductAction.Add(new Products() { Name = this.ProductName.Value.ToString() });
                        db.SaveChanges();
                    }
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
            this.ListShow.CollectionChanged += this.MyItemsSource_CollectionChanged;
        }
        void MyItemsSource_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            var x = sender as TrulyObservableCollection<ProductsShow>;
            if(x != null && this.ListShowDb.Count == x.Count)
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

                        using (var db = new ShoppingContext())
                        {
                            if (db.DutyAction.Any(p => p.Name == item.value.Name))
                            {
                                // Updating the db with the buyer name selected information
                                db.DutyAction.SingleOrDefault(p => p.Name == item.value.Name).Selected = item.value.Selected;
                                db.SaveChanges();
                            }
                            else
                            {
                                // Adding information into db with the buyer name selected
                                db.DutyAction.Add(new ProductsShow() { Name = item.value.Name, Selected = item.value.Selected });
                                db.SaveChanges();
                            }
                        }
                    }
                    else 
                    {
                        using (var db = new ShoppingContext())
                        {
                            // this is executed when shopping was done
                            var excuted = db.DutyAction.SingleOrDefault(g => g.Name == item.value.Name);

                            if (excuted != null && item.value.Executed != excuted.Executed)
                            {
                                // Changing information about execution
                                Console.WriteLine($" {item.value.Name}  was bought = {item.value.Executed}");
                                db.DutyAction.SingleOrDefault(g => g.Name == item.value.Name).Executed = item.value.Executed;
                                db.SaveChanges();
                            }
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
        void UpdateLists()
        {
            using (var db = new ShoppingContext())
            {
                foreach (var item in db.ProductAction)
                {
                    // Adding products to the list from the db
                    if(!this.List.Any(x=> x.Name == item.Name))
                        this.List.Add(item);                      
                }
                // Gettings number of products
                this.NumberofItems.Value = this.List.Count();

                foreach (var item in db.BuyersAction)
                {
                    if (!this.UserList.Any(x => x.BuyerId == item.BuyerId))
                    {
                        // Adding users to the list from the db
                        this.UserList.Add(item);
                    }
                }
                // Gettings number of buyers
                this.NumberofUsers.Value = this.UserList.Count();

                // Two default users if buers list is empty
                if(this.NumberofUsers.Value == 0)
                {
                    db.BuyersAction.Add(new Buyers  {FirstName = "Ana", SecondName = "Superb" });
                    db.BuyersAction.Add(new Buyers { FirstName = "Mike", SecondName = "EvenBetter" });
                    db.SaveChanges();
                    UpdateLists();
                }
                foreach (var item in this.List)
                {
                    var Li = new ObservableCollection<Buyers>();
                    foreach (var us in this.UserList)
                    {
                        // Gettings Buyers list from the db 
                        Li.Add(us);
                    }
                    if (!this.ListShow.Any(x => x.Name == item.Name))
                    {
                        if (db.DutyAction.Any(p => p.Name == item.Name))
                        {
                            // Get selection from the db and show
                            var selecteduser = db.DutyAction.SingleOrDefault(p => p.Name == item.Name).Selected;
                            this.ListShow.Add(new ProductsShow(new ReactiveProperty<Buyers>(this.UserList.SingleOrDefault(g => g.FirstName == selecteduser)), new ReactiveProperty<bool>(db.DutyAction.SingleOrDefault(p => p.Name == item.Name).Executed)) { Name = item.Name, UserList = Li });
                        }
                        else
                        {
                            // Get selection from the db and show
                            this.ListShow.Add(new ProductsShow { Name = item.Name, UserList = Li });
                        }
                    }
             
                }
            }
        }

       
    }
}
