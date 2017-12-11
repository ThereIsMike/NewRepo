using Microsoft.Azure.Mobile.Server;
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
  
    public class Status :  EntityData, IViewModelBase
    { 
        public string Name { get; set; }

        public string Selected { get; set; }
        public  ReactiveProperty<User> UserSelected { get; set; } = new ReactiveProperty<User>();

        public ObservableCollection<User> UserList { get; set; }

        public ReactiveProperty<bool> BuyExecuted { get; set; } = new ReactiveProperty<bool>();

        public bool Executed { get; set; }

        public Status()
        {
            this.UserSelected.Subscribe(x => {
                if (x!=null)this.Selected = x.FirstName;
                RaisePropertyChangedEvent(this.Name);
            });

            this.BuyExecuted.Subscribe(x => {
                this.Executed = x;
                RaisePropertyChangedEvent(this.Name);
            });
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProductsShow"/> class. Needed when selection has been already made.
        /// </summary>
        /// <param name="UsrSel">The usr sel.</param>
        /// <param name="BuyExe">The buy executable.</param>
        public Status(ReactiveProperty<User> UsrSel, ReactiveProperty<bool> BuyExe)
        {
            this.UserSelected = UsrSel;
            this.UserSelected.Subscribe(x => {
                if (x != null) this.Selected = x.FirstName;
                RaisePropertyChangedEvent(this.Name);
            });
            this.BuyExecuted = BuyExe;
            this.BuyExecuted.Subscribe(x => {
                this.Executed = x;
                RaisePropertyChangedEvent(this.Name);
            });
        }
        public Status(ReactiveProperty<User> UsrSel)
        {
            this.UserSelected = UsrSel;
            this.UserSelected.Subscribe(x => {
                if (x != null) this.Selected = x.FirstName;
                RaisePropertyChangedEvent(this.Name);
            });
          
        }

        public event PropertyChangedEventHandler PropertyChanged;

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
