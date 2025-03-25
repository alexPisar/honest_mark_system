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

        [JsonProperty(PropertyName = "createdTimestamp")]
        public long? CreatedTimestamp { get; set; }

        [JsonProperty(PropertyName = "declineReason")]
        public string DeclineReason { get; set; }

        [JsonProperty(PropertyName = "productGroup")]
        public string ProductGroup { get; set; }

        [JsonProperty(PropertyName = "paymentType")]
        public int PaymentType { get; set; }
    }
}
