﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace WebSystems.Models
{
    public class EdoProcessingErrorDescription
    {
        [JsonProperty(PropertyName = "CisNotExists")]
        public string[] Description { get; set; }

        [JsonProperty(PropertyName = "details")]
        public string Detail { get; set; }
    }
}
