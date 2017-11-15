using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp1
{
    public class ProductsShow : ViewModelBase
    {
        public string Name { get; set; }
        public ReactiveProperty<string> UserSelected { get; set; } = new ReactiveProperty<string>();

        public ObservableCollection<Buyers> UserList { get; set; }  
        

        public ProductsShow()
        {
            UserSelected.Subscribe(x => RaisePropertyChangedEvent("MyProperty"));
        }
    }
}
