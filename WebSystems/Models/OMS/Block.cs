using System;
using Newtonsoft.Json;

namespace WebSystems.Models.OMS
{
    public class Block
    {
        [JsonProperty(PropertyName = "blockId")]
        public string BlockId { get; set; }

        [JsonProperty(PropertyName = "quantity")]
        public int Quantity { get; set; }

        //[JsonProperty(PropertyName = "blockDateTime")]
        //public Int64 BlockDateTime { get; set; }
    }
}
