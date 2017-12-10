using Microsoft.Azure.Mobile.Server;
using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Web;

namespace taskitnowService.DataObjects
{
    public class Status : EntityData
    { 
        public string Name { get; set; }

        public string Selected { get; set; }
        public ReactiveProperty<User> UserSelected { get; set; } = new ReactiveProperty<User>();

        public ObservableCollection<User> UserList { get; set; }

        public ReactiveProperty<bool> BuyExecuted { get; set; } = new ReactiveProperty<bool>();

        public bool Executed { get; set; }
    }

}