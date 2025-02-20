using System;
using Newtonsoft.Json;

namespace WebSystems.Models.OMS
{
    public class OrdersList
    {
        [JsonProperty(PropertyName = "omsId")]
        public string OmsId { get; set; }

        [JsonProperty(PropertyName = "orderInfos")]
        public OrderInfo[] OrderInfos { get; set; }
    }
}
