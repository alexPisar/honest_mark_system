using System.Security.Cryptography.X509Certificates;
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
    }
}
