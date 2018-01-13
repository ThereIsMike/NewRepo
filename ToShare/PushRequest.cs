using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToShare
{
    public class PushRequest
    {
        [JsonProperty("action")]
        public string Action { get; set; }

        [JsonProperty("silent")]
        public bool Silent { get; set; }

        [JsonProperty("tags")]
        public string[] Tags { get; set; }

        [JsonProperty("text")]
        public string Text { get; set; }
    }
}