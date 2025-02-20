using System;
using Newtonsoft.Json;

namespace WebSystems.Models.OMS
{
    public class IdentificationCode
    {
        [JsonProperty(PropertyName = "code")]
        public string Code { get; set; }

        [JsonProperty(PropertyName = "quality")]
        public string Quality { get; set; }
    }
}
