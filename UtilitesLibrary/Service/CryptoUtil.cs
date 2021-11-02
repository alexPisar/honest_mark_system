using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Cryptography.WinApi;

namespace UtilitesLibrary.Service
{
    public class CryptoUtil
    {
        private WinApiCryptWrapper _crypto;

        public CryptoUtil()
        {
            _crypto = new WinApiCryptWrapper();
        }

        public CryptoUtil(X509Certificate2 certificate)
        {
            _crypto = new WinApiCryptWrapper(certificate);
        }

        public List<X509Certificate2> GetPersonalCertificates()
        {
            var certificates = _crypto.GetPersonalCertificates(true, true);

            certificates.AddRange(_crypto.GetPersonalCertificates(true, false));

            certificates = _crypto.GetCertificatesWithGostSignAlgorithm(certificates);

            return certificates;
        }

        public string ParseCertAttribute(string certData, string attributeName)
        {
            string result = String.Empty;
            try
            {
                if (certData == null || certData == "") return result;

                attributeName = attributeName + "=";

                if (!certData.Contains(attributeName)) return result;

                int start = certData.IndexOf(attributeName);

                if (start > 0 && !certData.Substring(0, start).EndsWith(" "))
                {
                    attributeName = " " + attributeName;

                    if (!certData.Contains(attributeName)) return result;
                }

                start = certData.IndexOf(attributeName) + attributeName.Length;

                int length = certData.IndexOf('=', start) == -1 ? certData.Length - start : certData.IndexOf(", ", start) - start;

                if (length == 0) return result;
                if (length > 0)
                {
                    result = certData.Substring(start, length);

                }
                else
                {
                    result = certData.Substring(start);
                }
                return result;

            }
            catch (Exception)
            {
                return result;
            }
        }

        public byte[] Sign(byte[] fileContent, bool isDetached)
        {
            return _crypto.Sign(fileContent, isDetached);
        }
    }
}
