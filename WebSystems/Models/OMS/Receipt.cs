using System;
using Newtonsoft.Json;

namespace WebSystems.Models.OMS
{
    public class Receipt
    {
        [JsonProperty(PropertyName = "resultDocId")]
        public string ResultDocId { get; set; }

        [JsonProperty(PropertyName = "resultDocDate")]
        public long ResultDocDate { get; set; }

        [JsonProperty(PropertyName = "sourceDocId")]
        public string SourceDocId { get; set; }

        [JsonProperty(PropertyName = "sourceDocDate")]
        public long SourceDocDate { get; set; }

        [JsonProperty(PropertyName = "state")]
        public string State { get; set; }

        [JsonProperty(PropertyName = "description")]
        public string Description { get; set; }

        [JsonProperty(PropertyName = "code")]
        public int Code { get; set; }

        [JsonProperty(PropertyName = "workflow")]
        public string Workflow { get; set; }

        [JsonProperty(PropertyName = "workflowVersion")]
        public int WorkflowVersion { get; set; }
    }
}
