using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp1
{
    public class MainViewModel
    {
        public int NumberofItems { get; set; }        
        public ObservableCollection<ShoppingList> List { get; set; } = new ObservableCollection<ShoppingList>();

        public ObservableCollection<Buyers> UserList { get; set; } = new ObservableCollection<Buyers>();

        public ReactiveProperty<bool> PushProduct { get; set; } = new ReactiveProperty<bool>();

        public ReactiveProperty<bool> ProductUpdated { get; set; } = new ReactiveProperty<bool>();

        public ReactiveProperty<string> ProductName { get; set; } = new ReactiveProperty<string>("Empty");

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
                        db.ShoppingAction.Add(new ShoppingList() { Name = this.ProductName.Value.ToString(), Assigned = new Buyers() { FirstName = "Michal", SecondName = "Kozik2" } });
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

            
        }

        void UpdateLists()
        {
            using (var db = new ShoppingContext())
            {

                foreach (var item in db.ShoppingAction)
                {
                    if(!this.List.Any(x=> x.Name == item.Name))
                        this.List.Add(item);

                }

                this.NumberofItems = this.List.Count();

                foreach (var item in db.BuyersAction)
                {
                    if (!this.UserList.Any(x => x.SecondName == item.SecondName))
                        this.UserList.Add(item);
                }
            }
        }
    }
}
