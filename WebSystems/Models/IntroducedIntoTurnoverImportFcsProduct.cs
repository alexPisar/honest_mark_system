using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace WebSystems.Models
{
    public class IntroducedIntoTurnoverImportFcsProduct
    {
        [JsonProperty(PropertyName = "cis")]
        public string Cis { get; set; }

        [JsonProperty(PropertyName = "color")]
        public string Color { get; set; }

        [JsonProperty(PropertyName = "productSize")]
        public string ProductSize { get; set; }

        [JsonProperty(PropertyName = "vsd_number")]
        public string VsdNumber { get; set; }

        [JsonProperty(PropertyName = "production_date")]
        public string ProductionDate { get; set; }
    }
}
