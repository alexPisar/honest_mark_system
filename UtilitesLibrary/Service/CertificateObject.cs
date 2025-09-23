using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography.X509Certificates;

namespace UtilitesLibrary.Service
{
    public class CertificateObject
    {
        private X509Certificate2 _certificate = null;
        private string _subjectName;
        private string _inn;
        private string _orgInn;

        public CertificateObject(X509Certificate2 certificate, CryptoUtil cryptoUtil = null)
        {
            _certificate = certificate;

            if(_certificate != null)
            {
                if (cryptoUtil == null)
                    cryptoUtil = new CryptoUtil(_certificate);

                _subjectName = cryptoUtil.ParseCertAttribute(_certificate.Subject, "CN");
                _inn = cryptoUtil.ParseCertAttribute(_certificate.Subject, "ИНН");
                _orgInn = cryptoUtil.GetOrgInnFromCertificate(_certificate); ;
            }
        }

        public string SubjectName => _subjectName;
        public X509Certificate2 Certificate => _certificate;
        public string Inn => _inn;
        public string OrgInn => _orgInn;
        public string Thumbprint => _certificate?.Thumbprint;
    }
}
