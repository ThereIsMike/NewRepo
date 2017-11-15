using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp1
{
    public class Products
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid ProductId { get; set; }

        [Required, MaxLength(20)]
        public string Name { get; set; }

  

    }
}
