using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            this.DataContext = new MainViewModel();
            InitializeComponent();
            
            using (var db = new ShoppingContext())
            {
                if(db.BuyersAction.Count() ==0)
                db.BuyersAction.Add(new Buyers() { FirstName = "Michal", SecondName = "Kozik" });
                if(db.ShoppingAction.Count() == 0)
                db.ShoppingAction.Add(new ShoppingList() { Name = "Butter" , Assigned = new Buyers() { FirstName = "Michal", SecondName = "Kozik2" } });
                db.SaveChanges();
            }
        }
    }
}
