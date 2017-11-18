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

        public ReactiveProperty<string> ProductName { get; set; } = new ReactiveProperty<string>("Empty");

        //public ReactiveProperty<Buyers> SelectedBuyer { get; set; } = new ReactiveProperty<Buyers>();


        public MainViewModel()
        {
            Subscriptions();
            UpdateLists();
            
        }

        void Subscriptions()
        {
            

            this.PushProduct.Subscribe(x => 
            { if (x)
                {
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

            Observable.Interval(TimeSpan.FromSeconds(0.5)).ObserveOnDispatcher().Subscribe(_ =>
            {
                if (this.ProductUpdated.Value)
                {
                    this.PushProduct.Value = !this.PushProduct.Value;
                    this.ProductUpdated.Value = false;

                        UpdateLists();
                  
                }
            });

            //this.SelectedBuyer.Select(x => (new ShoppingContext()).BuyersAction.Where(y => y.FirstName == x.FirstName)).Subscribe(z =>  Console.WriteLine(z.First().BuyerId.ToString()));

            this.ListShow.CollectionChanged += this.MyItemsSource_CollectionChanged;
            //this.ListShow.CollectionChanged += (this.CollectionChangedMethod);
        }
        void MyItemsSource_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            var x = sender as TrulyObservableCollection<ProductsShow>;
            if(x != null && this.ListShowDb.Count == x.Count)
                foreach (var item in x.Select((value, i) => new { i, value }))
                {
                    if (item.value.Selected != this.ListShowDb[item.i])
                    {
                        Console.WriteLine($"Assigned  { item.value.Selected} to buy { item.value.Name}" );
                        this.ListShowDb[item.i] = item.value.Selected;

                        using (var db = new ShoppingContext())
                        {
                            if (db.DutyAction.Any(p => p.Name == item.value.Name))
                            {
                                db.DutyAction.SingleOrDefault(p => p.Name == item.value.Name).Selected = item.value.Selected;
                                db.SaveChanges();
                            }
                            else
                            {
                                db.DutyAction.Add(new ProductsShow() { Name = item.value.Name, Selected = item.value.Selected });
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
        private void CollectionChangedMethod(object sender, NotifyCollectionChangedEventArgs e)
        {

            //different kind of changes that may have occurred in collection
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                Console.WriteLine("Add");
            }
            if (e.Action == NotifyCollectionChangedAction.Replace)
            {
                Console.WriteLine("Replace");
            }
            if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                Console.WriteLine("Remove");
            }
            if (e.Action == NotifyCollectionChangedAction.Move)
            {
                Console.WriteLine("Move");
            }
            if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                Console.WriteLine("Reset");
            }
        }
        private void YourCollection_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs args)
        {
           
        }
        void UpdateLists()
        {
            using (var db = new ShoppingContext())
            {

                foreach (var item in db.ProductAction)
                {
                    if(!this.List.Any(x=> x.Name == item.Name))
                        this.List.Add(item);
                        
                }

                this.NumberofItems.Value = this.List.Count();

                foreach (var item in db.BuyersAction)
                {
                    if (!this.UserList.Any(x => x.BuyerId == item.BuyerId))
                        this.UserList.Add(item);
                }

                this.NumberofUsers.Value = this.UserList.Count();

                if(this.NumberofUsers.Value == 0)
                {
                    db.BuyersAction.Add(new Buyers  {FirstName = "Anna", SecondName = "Kozminska" });
                    db.BuyersAction.Add(new Buyers { FirstName = "Michal", SecondName = "Kozminski" });
                    db.SaveChanges();
                    UpdateLists();
                }
                foreach (var item in this.List)
                {
                    var Li = new ObservableCollection<Buyers>();
                    foreach (var us in this.UserList)
                    {
                        Li.Add(us);
                    }
                    if (!this.ListShow.Any(x => x.Name == item.Name))
                    {
                        if (db.DutyAction.Any(p => p.Name == item.Name))
                        {
                            var selecteduser = db.DutyAction.SingleOrDefault(p => p.Name == item.Name).Selected;
                            this.ListShow.Add(new ProductsShow { Name = item.Name, UserList = Li, UserSelected = new ReactiveProperty<Buyers>(this.UserList.SingleOrDefault(g => g.FirstName == selecteduser)) });
                        }
                        else
                            this.ListShow.Add(new ProductsShow { Name = item.Name, UserList = Li });
                    }
             
                }
            }
        }

       
    }
}
