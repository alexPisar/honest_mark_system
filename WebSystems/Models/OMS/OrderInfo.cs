using System;
using Newtonsoft.Json;

namespace WebSystems.Models.OMS
{
    public class OrderInfo
    {
        [JsonProperty(PropertyName = "orderId")]
        public string OrderId { get; set; }

        [JsonProperty(PropertyName = "orderStatus")]
        public string OrderStatus { get; set; }

        [JsonProperty(PropertyName = "productGroup")]
        public string ProductGroup { get; set; }

        [JsonProperty(PropertyName = "paymentType")]
        public int PaymentType { get; set; }
    }
}
