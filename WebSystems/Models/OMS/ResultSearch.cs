using System;
using Newtonsoft.Json;

namespace WebSystems.Models.OMS
{
    public class ResultSearch
    {
        [JsonProperty(PropertyName = "docId")]
        public string DocId { get; set; }

        [JsonProperty(PropertyName = "documentType")]
        public string DocumentType { get; set; }

        [JsonProperty(PropertyName = "productGroup")]
        public string ProductGroup { get; set; }

        [JsonProperty(PropertyName = "status")]
        public string Status { get; set; }

        [JsonProperty(PropertyName = "createdTimestamp")]
        public int CreatedTimestamp { get; set; }
    }
}
