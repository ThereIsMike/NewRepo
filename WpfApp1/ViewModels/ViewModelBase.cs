using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp1
{
    interface IViewModelBase : INotifyPropertyChanged
    {
        event PropertyChangedEventHandler PropertyChanged;
        void RaisePropertyChangedEvent(string propertyName);
       
    }
}
