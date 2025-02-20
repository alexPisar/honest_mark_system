using System;
using Newtonsoft.Json;

namespace WebSystems.Models.OMS
{
    public class MarkedCodesFromReport
    {
        [JsonProperty(PropertyName = "orderId")]
        public string OrderId { get; set; }

        [JsonProperty(PropertyName = "sntins")]
        public IdentificationCode[] Sntins { get; set; }

        [JsonProperty(PropertyName = "usageType")]
        public string UsageType { get; set; }
    }
}
