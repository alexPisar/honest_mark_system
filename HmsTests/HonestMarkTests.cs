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
            var cert = crypto.GetCertificateWithPrivateKey("055DC379848800792FFF265DA204C8C101D293C8", false);
            HonestMarkClient.GetInstance().Authorization(cert);

            var inns = new List<WebSystems.Models.MarkCodeInfo>(HonestMarkClient.GetInstance().GetMarkCodesInfo(WebSystems.ProductGroupsEnum.Perfumery,
                new string[]
                {
                    "0104610044200449215InM%MXWdHRFa",
                    "0104610044200449215InhYq2edSvCt"
                }));
        }
    }
}
