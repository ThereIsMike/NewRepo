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

        public string Selected { get; set; }
        public ReactiveProperty<Buyers> UserSelected { get; set; } = new ReactiveProperty<Buyers>();

        public ObservableCollection<Buyers> UserList { get; set; }


        public ProductsShow()
        {
            this.UserSelected.Subscribe(x => {
                if (x!=null)this.Selected = x.FirstName;
                RaisePropertyChangedEvent(this.Name);
            });
        }
    }
}
