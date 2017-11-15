using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp1
{
    public class ProductList
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ItemId { get; set; }

        [Required]
        public Guid Name { get; set; }

        [Required]
        public Guid User { get; set; }


    }
}
