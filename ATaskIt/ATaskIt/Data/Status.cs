using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace ATaskIt.Data
{
    public class Status
    {
        public DateTimeOffset? CreatedAt { get; set; }

        public bool Deleted { get; set; }

        public bool Executed { get; set; }

        public string Id { get; set; }

        public string Name { get; set; }

        public string Selected { get; set; }

        public DateTimeOffset? UpdatedAt { get; set; }

        public byte[] Version { get; set; }
    }
}