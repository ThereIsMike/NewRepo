using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Mobile.Server;
using Microsoft.Azure.Mobile.Server.Tables;


namespace WpfApp1
{
    public class Item : EntityData
    {
        public string Name { get; set; }   
    }
    public class todoitem : EntityData
    {
        public string Text { get; set; }
        public bool Complete { get; set; }
    }
}
