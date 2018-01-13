using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ToShare
{
    public class DeviceInstallation
    {
        public DeviceInstallation()
        {
            this.Tags = new List<string>();
            this.Templates = new Dictionary<string, PushTemplate>();
        }

        [JsonProperty(PropertyName = "installationId")]
        public string InstallationId { get; set; }

        [JsonProperty(PropertyName = "platform")]
        public string Platform { get; set; }

        [JsonProperty(PropertyName = "pushChannel")]
        public string PushChannel { get; set; }

        [JsonProperty(PropertyName = "tags")]
        public List<string> Tags { get; set; }

        [JsonProperty(PropertyName = "templates")]
        public Dictionary<string, PushTemplate> Templates { get; set; }
    }

    public class PushTemplate
    {
        [JsonProperty(PropertyName = "body")]
        public string Body { get; set; }
    }
}