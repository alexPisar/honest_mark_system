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
            var cert = crypto.GetCertificateWithPrivateKey("439C9C0937713DEEA5334DB7228585A55B11498C", false);
            HonestMarkClient.GetInstance().Authorization(cert, "2539108495");

            var markedCodeInfos = new List<WebSystems.Models.MarkCodeInfo>(HonestMarkClient.GetInstance().GetMarkCodesInfo(WebSystems.ProductGroupsEnum.None,
                new string[]
                {
                    "0104610044201415215SJLvja.jJLYI",
                    "0104610044201668215eSx.Z+2)YgUc",
                    "0104610044201699215WFxnznQ)aXV;",
                    "0104610044201743215BQb.dq,'Ah.m"
                }));

            var aggregatedCodes = new List<string>(HonestMarkClient.GetInstance().GetAggregatedCodes(WebSystems.ProductGroupsEnum.None,
                new string[]
                {
                    "246100002122956389",
                    "246100002117691578"
                }));
        }

        [TestMethod]
        public void GetOrderMarkedCodesTest()
        {
            var crypto = new WinApiCryptWrapper();
            var cert = crypto.GetCertificateWithPrivateKey("6AFD4AA04C6B519162F18C5E13043B2C1F3C5B0B", false);
            var barCode = "4011669330632";

            try
            {
                //var content = Encoding.UTF8.GetString(Convert.FromBase64String("eyJAY2xhc3MiOiJjb20uZXF1aXJvbi5zaXRlbWFuYWdlci5hcGkudjIubW9kZWwucmVwb3J0LlV0aWxpc2F0aW9uUmVwb3J0RHRvQ2hlbWlzdHJ5Iiwic250aW5zIjpbIjAxMDQwMTE2NjkzMzA2MzIyMTVqKm5RTlx1MDAxRDkxRUUxMFx1MDAxRDkyK2k5d2RwMjhNWDVsRGJxUFExSGFxM05pQmpjRksySmJMVmtLT2FxYUg1az0iLCIwMTA0MDExNjY5MzMwNjMyMjE1MUYmciZcdTAwMUQ5MUVFMTBcdTAwMUQ5MmxVWlNlNHloQXVLZlNmVi80RTlVdkl3amJrazE3VlpDam1FV3VKbitpU1k9Il0sInVzYWdlVHlwZSI6IlBSSU5URUQifQ=="));
                OrderManagementStationClient.GetInstance().Authorization(cert, "84869e39-1e44-4fda-8b57-171f6e277e1f", "be1e46af-6d88-41d8-89b8-577a9884aab1", "2536090987");
                OrderManagementStationClient.GetInstance().GetBlockIdsFromOrder("7f9600e7-7f05-4029-b039-385c14e36b85", $"0{barCode}");
                //OrderManagementStationClient.GetInstance().GetMarkedCodesFromReport("f0d053cc-91ad-4b33-b549-b6d4bd71d4bc");
                var order = OrderManagementStationClient.GetInstance().GetOrdersList().OrderInfos.FirstOrDefault(o => o.OrderId == "7f9600e7-7f05-4029-b039-385c14e36b85");
                //var markedCodes = OrderManagementStationClient.GetInstance().GetMarkedCodes("7f9600e7-7f05-4029-b039-385c14e36b85", 1, $"0{barCode}");
                var markedCodes = OrderManagementStationClient.GetInstance().GetMarkedCodesRetry("beaf72f1-8390-4e7d-9e74-680365ec6679").Codes.ToList();
                //markedCodes.AddRange(OrderManagementStationClient.GetInstance().GetMarkedCodesRetry("24560a0c-d837-4dfa-aa6d-3f450b2bf0cb").Codes);
                //var result = OrderManagementStationClient.GetInstance().SendReportForApplication(order.ProductGroup, markedCodes);
                //markedCodes = OrderManagementStationClient.GetInstance().GetMarkedCodesRetry("fd4d47be-89d5-4f07-92c3-f0cdfef47677");
                //markedCodes = OrderManagementStationClient.GetInstance().GetMarkedCodesRetry("beaf72f1-8390-4e7d-9e74-680365ec6679");
            }
            catch (System.Net.WebException webEx)
            {

            }
            catch (Exception ex)
            {

            }
        }
    }
}
