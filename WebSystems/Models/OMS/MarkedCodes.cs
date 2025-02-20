using System;
using Newtonsoft.Json;

namespace WebSystems.Models.OMS
{
    public class MarkedCodes
    {
        [JsonProperty(PropertyName = "omsId")]
        public string OmsId { get; set; }

        [JsonProperty(PropertyName = "codes")]
        public string[] Codes { get; set; }

        [JsonProperty(PropertyName = "blockId")]
        public string BlockId { get; set; }
    }
}
