using System;
using Newtonsoft.Json;

namespace WebSystems.Models.OMS
{
    public class BufferInfo
    {
        [JsonProperty(PropertyName = "availableCodes")]
        public int AvailableCodes { get; set; }

        [JsonProperty(PropertyName = "bufferStatus")]
        public string BufferStatus { get; set; }

        [JsonProperty(PropertyName = "gtin")]
        public string Gtin { get; set; }

        [JsonProperty(PropertyName = "leftInBuffer")]
        public int LeftInBuffer { get; set; }

        [JsonProperty(PropertyName = "poolsExhausted")]
        public bool PoolsExhausted { get; set; }

        [JsonProperty(PropertyName = "rejectionReason")]
        public string RejectionReason { get; set; }

        [JsonProperty(PropertyName = "totalCodes")]
        public int TotalCodes { get; set; }

        [JsonProperty(PropertyName = "totalPassed")]
        public int TotalPassed { get; set; }

        [JsonProperty(PropertyName = "unavailableCodes")]
        public int UnavailableCodes { get; set; }

        [JsonProperty(PropertyName = "expiredDate")]
        public long? ExpiredDate { get; set; }

        [JsonProperty(PropertyName = "productionOrderId")]
        public string ProductionOrderId { get; set; }

        [JsonProperty(PropertyName = "cisType")]
        public string CisType { get; set; }

        [JsonProperty(PropertyName = "templateId")]
        public int TemplateId { get; set; }
    }
}
