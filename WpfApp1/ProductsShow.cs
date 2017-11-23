using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp1
{
    public class ProductsShow : ViewModelBase
    {   [Key]
        public string Name { get; set; }

        public string Selected { get; set; }
        public  ReactiveProperty<Buyers> UserSelected { get; set; } = new ReactiveProperty<Buyers>();

        public ObservableCollection<Buyers> UserList { get; set; }

        public ReactiveProperty<bool> BuyExecuted { get; set; } = new ReactiveProperty<bool>();

        public bool Executed { get; set; }


        public ProductsShow()
        {
            UserSelected.Subscribe(x => {
                if (x!=null)this.Selected = x.FirstName;
                RaisePropertyChangedEvent(this.Name);
            });

            BuyExecuted.Subscribe(x => {
                this.Executed = x;
                RaisePropertyChangedEvent(this.Name);
            });
        }

        public ProductsShow(ReactiveProperty<Buyers> UsrSel, ReactiveProperty<bool> BuyExe)
        {
            this.UserSelected = UsrSel;
            UserSelected.Subscribe(x => {
                if (x != null) this.Selected = x.FirstName;
                RaisePropertyChangedEvent(this.Name);
            });
            this.BuyExecuted = BuyExe;
            this.BuyExecuted.Subscribe(x => {
                this.Executed = x;
                RaisePropertyChangedEvent(this.Name);
            });
        }
        public ProductsShow(ReactiveProperty<Buyers> UsrSel)
        {
            this.UserSelected = UsrSel;
            UserSelected.Subscribe(x => {
                if (x != null) this.Selected = x.FirstName;
                RaisePropertyChangedEvent(this.Name);
            });
          
        }
    }
}
