using System;
using Newtonsoft.Json;

namespace WebSystems.Models.OMS
{
    public class ReportForApplicationResponse
    {
        [JsonProperty(PropertyName = "omsId")]
        public string OmsId { get; set; }

        [JsonProperty(PropertyName = "reportId")]
        public string ReportId { get; set; }
    }
}
