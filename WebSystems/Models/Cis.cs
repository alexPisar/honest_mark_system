using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace WebSystems.Models
{
    [JsonObject]
    public class Cis
    {
        [JsonExtensionData]
        public Dictionary<string, Newtonsoft.Json.Linq.JToken> Cises { get; set; }

        public string[] Codes { get; set; }
    }
}
