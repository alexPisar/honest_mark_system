using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace WebSystems.Models
{
    public class WithdrawalFromTurnoverDocument : IDocument
    {
        [JsonProperty(PropertyName = "inn")]
        public string Inn { get; set; }

        [JsonProperty(PropertyName = "action_date")]
        public string ActionDateStr { get; set; }

        [JsonProperty(PropertyName = "action")]
        [JsonConverter(typeof(Newtonsoft.Json.Converters.StringEnumConverter))]
        public ReasonOfWithdrawalFromTurnover Action { get; set; }

        [JsonProperty(PropertyName = "withdrawal_type_other")]
        public string WithdrawalTypeOther { get; set; }

        [JsonProperty(PropertyName = "state_contract_id")]
        public string StateContractId { get; set; }

        [JsonProperty(PropertyName = "document_type")]
        public string DocumentType { get; set; }

        [JsonProperty(PropertyName = "document_number")]
        public string DocumentNumber { get; set; }

        [JsonProperty(PropertyName = "document_date")]
        public string DocumentDateStr { get; set; }

        [JsonProperty(PropertyName = "primary_document_custom_name")]
        public string PrimaryDocumentCustomName { get; set; }

        [JsonProperty(PropertyName = "destination_country_code")]
        public string DestinationCountryCode { get; set; }

        [JsonProperty(PropertyName = "importer_id")]
        public string ImporterId { get; set; }

        [JsonProperty(PropertyName = "kkt_number")]
        public string KktNumber { get; set; }

        [JsonProperty(PropertyName = "products")]
        public WithdrawalFromTurnoverDetail[] Products { get; set; }
    }
}
