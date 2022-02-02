using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace WebSystems.Models
{
    public class WithdrawalFromTurnoverDetail
    {
        [JsonProperty(PropertyName = "cis")]
        public string Cis { get; set; }

        [JsonProperty(PropertyName = "product_cost")]
        public string ProductCost { get; set; }
    }
}
