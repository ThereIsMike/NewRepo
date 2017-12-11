using Microsoft.Azure.Mobile.Server;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp1
{
    public class User : EntityData
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }
        [Required,  MaxLength(10)]
        public string FirstName { get; set; }

        [Required, MaxLength(10)]
        public string SecondName { get; set; }
        public string Mail { get; set; }
        public string PhoneNumber { get; set; }


    }
}
