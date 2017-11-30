using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace WpfApp1
{
    public class UserViewModel
    {
        public IReactiveProperty<string> DbId { get; set; } = new ReactiveProperty<string>();

        public IReactiveProperty<string> DbPassword { get; set; } = new ReactiveProperty<string>();

        public IReactiveProperty<string> NewUser { get; set; } = new ReactiveProperty<string>();

        public IReactiveProperty<string> NewPassword { get; set; } = new ReactiveProperty<string>();

        public ReactiveProperty<bool> DbConnection { get; set; } = new ReactiveProperty<bool>();

        public IReactiveProperty<bool> EnableRemoteLogin { get; set; } = new ReactiveProperty<bool>();

        public ReactiveCommand<object> Login { get; set; } = new ReactiveCommand<object>();

        public string DbDataSource { get; set; } =  "empty";

        public bool isLoaded { get; set; } = false;

        private static UserViewModel _instance = new UserViewModel();
        public static UserViewModel Instance { get { return _instance; } }


        public UserViewModel()
        {
            Subscriptions();
            _instance = this;
            isLoaded = true;
        }

        private void Subscriptions()
        {
            this.DbConnection.Subscribe(x =>
            {
                    EnableRemoteLogin.Value = !x;

            });

            this.Login.Subscribe(x => {
                var passwd = x as PasswordBox;
                Console.WriteLine($"Remote is {EnableRemoteLogin.Value}");
                Console.WriteLine($"Login as {DbId.Value} and password {passwd.Password}");
                this.DbPassword.Value = passwd.Password;

                if (true)
                {


                    using (var db = new ShoppingContext(DynamicConnectionString(this.DbPassword.Value)))
                    {
                        db.Database.Exists();

                        Console.WriteLine($"Exists {db.Database.Exists()}");
                    }
                }
            });



        }
        public string DynamicConnectionString(string pw)
        {
            SqlConnection myConnection = new SqlConnection();

            SqlConnectionStringBuilder myBuilder = new SqlConnectionStringBuilder();
            myBuilder.UserID = DbId.Value + "@mylearningcurve.database.windows.net";
            myBuilder.Password = pw;
            myBuilder.InitialCatalog = "Shopping";
            myBuilder.DataSource = "tcp:mylearningcurve.database.windows.net,1433";
            myBuilder.ConnectTimeout = 30;
            myBuilder.PersistSecurityInfo = false;
            myBuilder.Encrypt = true;
            myBuilder.TrustServerCertificate = false;
            myBuilder.MultipleActiveResultSets = false;
            this.DbDataSource = myBuilder.ConnectionString;
            return myBuilder.ConnectionString;
        }
    }

    
}
