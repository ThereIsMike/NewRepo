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
        public DbSet<Buyers> BuyersAction { get; set; }

        public DbSet<Products> ProductAction { get; set; }

        public DbSet<ProductsShow> DutyAction { get; set; }
    }
}
