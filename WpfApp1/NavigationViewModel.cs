using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace WpfApp1
{
    class NavigationViewModel : INotifyPropertyChanged
    {
        public ICommand UserCommand { get; set; }
        public ICommand MainCommand { get; set; }
        private object selectedViewModel;
        public object SelectedViewModel
        {
            get { return this.selectedViewModel; }
            set { this.selectedViewModel = value; OnPropertyChanged("SelectedViewModel"); }
        }

        public NavigationViewModel()
        {
            this.UserCommand = new BaseCommand(this.OpenUser);
            this.MainCommand = new BaseCommand(this.OpenMain);
        }
        private void OpenUser(object obj)
        {
            if (UserViewModel.Instance.DbDataSource == "empty")
                this.SelectedViewModel = new UserViewModel();
            else
                this.SelectedViewModel = UserViewModel.Instance;
        }
        private void OpenMain(object obj)
        {
            this.SelectedViewModel = new MainViewModel();
        }
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
            }
        }
    }
}