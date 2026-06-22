using System;
using Newtonsoft.Json;

namespace WebSystems.Models
{
    public class ProductSearchIdentifiedBy
    {
        [JsonProperty(PropertyName = "value")]
        public string Value { get; set; }

        [JsonProperty(PropertyName = "type")]
        public string Type { get; set; }

        [JsonProperty(PropertyName = "multiplier")]
        public int Multiplier { get; set; }

        [JsonProperty(PropertyName = "level")]
        public string Level { get; set; }
    }
}
