using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp1
{
    public class MainViewModel
    {
        public int NumberofItems { get; set; }        
        public ObservableCollection<ShoppingList> List { get; set; } = new ObservableCollection<ShoppingList>();

        public ObservableCollection<Buyers> UserList { get; set; } = new ObservableCollection<Buyers>();
        public MainViewModel()
        {
            using (var db = new ShoppingContext())
            {
    
                foreach (var item in db.ShoppingAction)
                {
                    this.List.Add(item);
                }

                this.NumberofItems = this.List.Count();
 
                foreach (var item in db.BuyersAction)
                {
                    this.UserList.Add(item);
                }
            }
            
        }
    }
}
