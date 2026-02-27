using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace WebSystems.Models
{
    public class IntroducedIntoTurnoverOutsideEAEC : IDocument
    {
        [JsonProperty(PropertyName = "participant_inn")]
        public string ParticipantInn { get; set; }

        [JsonProperty(PropertyName = "declaration_number")]
        public string DeclarationNumber { get; set; }

        [JsonProperty(PropertyName = "declaration_date")]
        public string DeclarationDate { get; set; }

        [JsonProperty(PropertyName = "customs_code")]
        public string CustomsCode { get; set; }

        [JsonProperty(PropertyName = "decision_code")]
        public string DecisionCode { get; set; }

        [JsonProperty(PropertyName = "products")]
        public IntroducedIntoTurnoverOutsideEaecProduct[] Products { get; set; }
    }
}
