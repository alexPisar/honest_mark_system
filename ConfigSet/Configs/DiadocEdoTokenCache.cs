﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfigSet.Configs
{
    public class DiadocEdoTokenCache : Configuration<DiadocEdoTokenCache>
    {
        public DiadocEdoTokenCache(string AuthToken, string Creator, string PartyId, DateTime? edoLastDocDateTime = null)
        {
            this.PartyId = PartyId;
            Token = AuthToken;
            TokenCreator = Creator;
            TokenCreationDate = DateTime.Now;
            TokenExpirationDate = DateTime.Now.AddHours(12);

            if (edoLastDocDateTime != null)
                EdoLastDocDateTime = edoLastDocDateTime.Value;
        }

        public DiadocEdoTokenCache() { }
        public string PartyId { get; set; }
        public string Token { get; set; }
        public string TokenCreator { get; set; }
        public string ActualBoxId { get; set; }
        public DateTime TokenCreationDate { get; set; }
        public DateTime TokenExpirationDate { get; set; }
        public DateTime EdoLastDocDateTime { get; set; }
    }
}
