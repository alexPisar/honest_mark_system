using System;
using Newtonsoft.Json;


namespace WebSystems.Models.OMS
{
    public class Product
    {
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "fullName")]
        public string FullName { get; set; }

        [JsonProperty(PropertyName = "brand")]
        public string Brand { get; set; }

        [JsonProperty(PropertyName = "productGroup")]
        public int ProductFroup { get; set; }
    }
}
