using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace WebSystems.Models
{
    public class IntroducedIntoTurnoverOutsideEaecProduct
    {
        [JsonProperty(PropertyName = "uit_code")]
        public string UitCode { get; set; }

        [JsonProperty(PropertyName = "uitu_code")]
        public string UituCode { get; set; }

        [JsonProperty(PropertyName = "tnved_code")]
        public string TnvedCode { get; set; }

        [JsonProperty(PropertyName = "certificate_type")]
        public string CertificateType { get; set; }

        [JsonProperty(PropertyName = "certificate_number")]
        public string CertificateNumber { get; set; }

        [JsonProperty(PropertyName = "certificate_date")]
        public string CertificateDate { get; set; }

        [JsonProperty(PropertyName = "vsd_number")]
        public string VsdNumber { get; set; }
    }
}
