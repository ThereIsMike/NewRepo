using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp1
{
    class ShoppingContext : DbContext 
    {
        public ShoppingContext(string nameOrConnectionString) : base(nameOrConnectionString)
        {
        }

        public DbSet<User> BuyersAction { get; set; }

        public DbSet<Item> ProductAction { get; set; }

        public DbSet<Status> DutyAction { get; set; }
       
    }
}
