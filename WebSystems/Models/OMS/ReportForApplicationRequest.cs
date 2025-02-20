using System;
using Newtonsoft.Json;

namespace WebSystems.Models.OMS
{
    public class ReportForApplicationRequest
    {
        [JsonProperty(PropertyName = "productGroup")]
        public string ProductGroup { get; set; }

        [JsonProperty(PropertyName = "sntins")]
        public string[] Sntins { get; set; }
    }
}
