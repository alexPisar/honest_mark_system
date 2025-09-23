using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography.X509Certificates;
using DataContextManagementUnit.DataAccess.Contexts.Abt;
using UtilitesLibrary.ModelBase;
using UtilitesLibrary.Service;

namespace OmsQrCodesMakerApp.Models
{
    public class AuthModel : ViewModelBase
    {
        private List<ViewModels.PowerOfAttorney> _powerOfAttorneys;
        private List<ViewModels.OmsOrganization> _allOrganizations;
        private CryptoUtil _cryptoUtil = new CryptoUtil();

        public void Init()
        {
            _powerOfAttorneys = GetPowerOfAttorneysFromDb();
            _allOrganizations = GetOrganizations();
            Certificates = _cryptoUtil.GetPersonalCertificates().OrderByDescending(c => c.NotBefore)?
                .Where(cert => _cryptoUtil.IsCertificateValid(cert) && cert.NotAfter > DateTime.Now)?
                .Select(c => new CertificateObject(c, _cryptoUtil))?.ToList();
        }

        public List<CertificateObject> Certificates { get; set; }
        public CertificateObject SelectedCertificate { get; set; }

        public List<ViewModels.OmsOrganization> Organizations { get; set; }
        public ViewModels.OmsOrganization SelectedOrganization { get; set; }

        public void SelectedCertificateChanged()
        {
            if (SelectedCertificate?.Certificate == null)
                return;

            var certInn = SelectedCertificate.Inn;
            var certOrgInn = SelectedCertificate.OrgInn;

            if (certInn == certOrgInn || string.IsNullOrEmpty(certOrgInn))
            {
                Organizations = (from org in _allOrganizations
                                 join p in _powerOfAttorneys on org.Inn equals p.OrgInn
                                 where p.PersonInn == certInn
                                 select org)?.ToList();
            }
            else if(!string.IsNullOrEmpty(certOrgInn))
            {
                Organizations = _allOrganizations.Where(o => o.Inn == certOrgInn).ToList();
            }

            OnPropertyChanged("Organizations");

            SelectedOrganization = null;
            if (Organizations.Count == 1)
                SelectedOrganization = Organizations.First();

            OnPropertyChanged("SelectedOrganization");
        }

        public List<ViewModels.PowerOfAttorney> GetPowerOfAttorneysFromDb()
        {
            List<ViewModels.PowerOfAttorney> powerOfAttorneys = null;
            using (var abt = new AbtDbContext())
            {
                powerOfAttorneys = (from a in abt.RefAuthoritySignDocuments
                                    where a.EmchdId != null
                                    join r in abt.RefCustomers on a.IdCustomer equals r.Id
                                    select new ViewModels.PowerOfAttorney
                                    {
                                        PersonSurname = a.Surname,
                                        PersonName = a.Name,
                                        PersonPatronimycSurname = a.PatronymicSurname,
                                        PersonPosition = a.Position,
                                        PersonInn = a.Inn,
                                        EmchdId = a.EmchdId,
                                        EmchdBeginDate = a.EmchdBeginDate,
                                        EmchdEndDate = a.EmchdEndDate,
                                        OrgName = r.Name,
                                        OrgInn = r.Inn
                                    }).ToList();
            }
            return powerOfAttorneys?.ToList();
        }

        public List<ViewModels.OmsOrganization> GetOrganizations()
        {
            var finDbWebClient = WebSystems.WebClients.FinDbWebClient.GetInstance();
            var orgs = finDbWebClient.GetApplicationConfigParameter<ViewModels.OmsOrganization[]>("OmsQrCodesMakerApp", "OmsConnections");

            return orgs?.ToList();
        }
    }
}
