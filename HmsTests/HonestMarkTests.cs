using System;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;
using Cryptography.WinApi;
using WebSystems.WebClients;

namespace HmsTests
{
    [TestClass]
    public class HonestMarkTests
    {
        [TestMethod]
        public void GetStatusTest()
        {
            var crypto = new WinApiCryptWrapper();
            var cert = crypto.GetCertificateWithPrivateKey("1AE6FE62C7DEE1C4CA5AAAF9A9B33AFA95640753", false);
            HonestMarkClient.GetInstance().Authorization(cert);

            var status = HonestMarkClient.GetInstance().GetEdoDocumentProcessInfo("DP_PRANNUL_2AE97D403BD-870F-4CD7-87EE-1F384836B637_2BM-2538150215-253801001-201605120119377915440_20220310_b35583e8-2a79-480e-bbb4-5f62d2630595");
        }

        [TestMethod]
        public void GetMarkedCodesTest()
        {
            var crypto = new WinApiCryptWrapper();
            var cert = crypto.GetCertificateWithPrivateKey("049D840F2D5C8A57A49FD91768936556AC41EEB2", false);
            HonestMarkClient.GetInstance().Authorization(cert);

            var markedCodeInfos = new List<WebSystems.Models.MarkCodeInfo>(HonestMarkClient.GetInstance().GetMarkCodesInfo(WebSystems.ProductGroupsEnum.None,
                new string[]
                {
                    "0104623721623660215dqmEEEwUBsXI",
                    "0104623721623660215dSUa:P'Xo*Gx",
                    "0104610044200487215CgCcY!SQ*XpD",
                    "0104607942315252215546729200318"
                }));

            var aggregatedCodes = new List<string>(HonestMarkClient.GetInstance().GetAggregatedCodes(WebSystems.ProductGroupsEnum.None,
                new string[]
                {
                    "246100002122956389",
                    "246100002117691578"
                }));
        }
    }
}
