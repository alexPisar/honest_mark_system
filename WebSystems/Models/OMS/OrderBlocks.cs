using System;
using Newtonsoft.Json;


namespace WebSystems.Models.OMS
{
    public class OrderBlocks
    {
        [JsonProperty(PropertyName = "omsId")]
        public string OmsId { get; set; }

        [JsonProperty(PropertyName = "orderId")]
        public string OrderId { get; set; }

        [JsonProperty(PropertyName = "gtin")]
        public string Gtin { get; set; }

        [JsonProperty(PropertyName = "blocks")]
        public Block[] Blocks { get; set; }
    }
}
