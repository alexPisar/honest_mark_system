using System;
using Newtonsoft.Json;

namespace WebSystems.Models.OMS
{
    public class DocumentContent
    {
        [JsonProperty(PropertyName = "docId")]
        public string DocId { get; set; }

        [JsonProperty(PropertyName = "productGroup")]
        public string ProductGroup { get; set; }

        [JsonProperty(PropertyName = "documentType")]
        public string DocumentType { get; set; }

        [JsonProperty(PropertyName = "status")]
        public string Status { get; set; }

        [JsonProperty(PropertyName = "createdTimestamp")]
        public long CreatedTimeStamp { get; set; }

        [JsonProperty(PropertyName = "content")]
        public string Content { get; set; }
    }
}
