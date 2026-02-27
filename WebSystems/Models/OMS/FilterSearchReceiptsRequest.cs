using System;
using Newtonsoft.Json;

namespace WebSystems.Models.OMS
{
    public class FilterSearchReceiptsRequest
    {
        [JsonProperty(PropertyName = "startCreateDocDate")]
        public long? StartCreateDocDate { get; set; }

        [JsonProperty(PropertyName = "endCreateDocDate")]
        public long? EndCreateDocDate { get; set; }

        [JsonProperty(PropertyName = "orderIds")]
        public string[] OrderIds { get; set; }

        [JsonProperty(PropertyName = "sourceDocIds")]
        public string[] SourceDocIds { get; set; }

        [JsonProperty(PropertyName = "resultDocIds")]
        public string[] ResultDocIds { get; set; }

        [JsonProperty(PropertyName = "resultCodes")]
        public int[] ResultCodes { get; set; }

        [JsonProperty(PropertyName = "productGroups")]
        public string[] ProductGroups { get; set; }

        [JsonProperty(PropertyName = "workflowTypes")]
        public string[] WorkflowTypes { get; set; }
    }
}
