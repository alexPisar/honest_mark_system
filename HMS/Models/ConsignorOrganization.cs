using System.Security.Cryptography.X509Certificates;
using System.Collections.Generic;
using UtilitesLibrary.Service;
using WebSystems;

namespace HonestMarkSystem.Models
{
    public class ConsignorOrganization
    {
        public X509Certificate2 Certificate { get; set; }
        public WebSystems.Systems.HonestMarkSystem HonestMarkSystem { get; set; }
        public CryptoUtil CryptoUtil { get; set; }
        public IEdoSystem EdoSystem { get; set; }
        public string OrgInn { get; set; }
        public string OrgKpp { get; set; }
        public string OrgName { get; set; }
        public string EdoId { get; set; }
        public List<string> ShipperOrgInns { get; set; }
        public string EmchdId { get; set; }
        public System.DateTime? EmchdBeginDate { get; set; } 
        public string EmchdPersonSurname { get; set; }
        public string EmchdPersonName { get; set; }
        public string EmchdPersonPatronymicSurname { get; set; }
        public string EmchdPersonPosition { get; set; }
        public string EmchdPersonInn { get; set; }
    }
}
