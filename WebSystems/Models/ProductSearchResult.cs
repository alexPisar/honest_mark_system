using System;
using Newtonsoft.Json;

namespace WebSystems.Models
{
    public class ProductSearchResult
    {
        [JsonProperty(PropertyName = "apiversion")]
        public int Apiversion { get; set; }

        [JsonProperty(PropertyName = "result")]
        public ProductSearchInfo[] Result { get; set; }
    }
}
