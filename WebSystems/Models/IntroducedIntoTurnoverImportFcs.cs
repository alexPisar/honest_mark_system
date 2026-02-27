using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace WebSystems.Models
{
    public class IntroducedIntoTurnoverImportFcs : IDocument
    {
        [JsonProperty(PropertyName = "trade_participant_inn")]
        public string TradeParticipantInn { get; set; }

        [JsonProperty(PropertyName = "trade_participant_kpp")]
        public string TradeParticipantKpp { get; set; }

        [JsonProperty(PropertyName = "declaration_number")]
        public string DeclarationNumber { get; set; }

        [JsonProperty(PropertyName = "declaration_date")]
        public string DeclarationDate { get; set; }

        [JsonProperty(PropertyName = "production_date")]
        public string ProductionDate { get; set; }

        [JsonProperty(PropertyName = "products_list")]
        public IntroducedIntoTurnoverImportFcsProduct[] Products { get; set; }
    }
}
