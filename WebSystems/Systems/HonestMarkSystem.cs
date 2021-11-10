using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography.X509Certificates;
using WebSystems.WebClients;

namespace WebSystems.Systems
{
    public class HonestMarkSystem
    {
        private X509Certificate2 _certificate;

        public HonestMarkSystem(X509Certificate2 certificate)
        {
            _certificate = certificate;
        }

        public bool Authorization()
        {
            return HonestMarkClient.GetInstance().Authorization(_certificate);
        }

        public void GetCodesByThePiece(IEnumerable<string> sourceCodes,
            List<KeyValuePair<string, string>> resultCodes)
        {
            var markedCodesInfo = HonestMarkClient.GetInstance()
                .GetMarkCodesInfo(ProductGroupsEnum.Perfumery, sourceCodes.ToArray());

            Func<string, KeyValuePair<string, string>> predicate = s =>
            {
                if (s.Length == 31)
                {
                    var barCode = s.Substring(0, 16).TrimStart('0', '1').TrimStart('0');
                    return new KeyValuePair<string, string>(s, barCode);
                }
                else
                    return new KeyValuePair<string, string>(s, string.Empty);
            };

            IEnumerable<KeyValuePair<string, string>> codes = sourceCodes.Select(s => predicate(s));


            while (markedCodesInfo.Any(m => 
            {
                if (m?.CisInfo == null)
                    return false;

                if (m.CisInfo.Children == null)
                    return false;

                return m.CisInfo.Children.Count() > 0;
            }))
            {
                codes = markedCodesInfo.SelectMany(m => m?.CisInfo?.Children?.ToList())?
                    .Select(c => predicate(c));

                markedCodesInfo = HonestMarkClient.GetInstance()
                    .GetMarkCodesInfo(ProductGroupsEnum.Perfumery, codes?.Select(c => c.Key)?.ToArray() ?? new string[] { });
            }

            resultCodes.AddRange(codes);
        }
    }
}
