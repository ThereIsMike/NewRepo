using Microsoft.Azure.Mobile.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace taskitnowService.DataObjects
{
    public class User : EntityData
    {
        public string FirstName { get; set; }
        public string SecondName { get; set; }
        public string Mail { get; set; }
        public string PhoneNumber { get; set; }

    }
}