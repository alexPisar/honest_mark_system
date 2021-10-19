using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace WebSystems.Models
{
    public class EdoLiteDocumentList
    {
        [JsonProperty(PropertyName = "count")]
        public int Count { get; set; }

        [JsonProperty(PropertyName = "items")]
        public EdoLiteDocuments[] Items { get; set; }
    }
}
