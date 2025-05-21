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
        public void CheckUcdTest()
        {
            decimal idDoc = 1150583700;
            string[] deletedMarkedCodes = new string[] { "0104607066904233215uRbXcjp:Qsd=", "0104607066904233215jQB:fu=FRpHP", "0104607066904233215eEbNJX>%eS4s",
                "0104607066904233215e1jnGBRl8soL", "0104607066904233215eK3,>_ngMRDg", "0104607066904233215esE(DW-Kwhqe" };

            var xmlDocument = new System.Xml.XmlDocument();
            xmlDocument.Load("C:\\Users\\systech\\Desktop\\ON_NKORSCHFDOPPRMARK_2BM-2539108495-253901001-201407110842566644066_2BM-5035022477-2012052808202463102630000000000_20250516_589283ad-e8f8-406c-8797-2978bb8772cb.xml");

            var report = UtilitesLibrary.Service.Xml.DeserializeEntity<Reporter.XsdClasses.OnNkorschfdoppr.Файл>(xmlDocument.OuterXml);

            foreach (var product in report.Документ.ТаблКСчФ.СведТов)
            {
                var markedCodesBefore = product.НомСредИдентТовДо[0].Items;
                var markedCodesAfter = product.НомСредИдентТовПосле[0].Items;
                var barCode = markedCodesBefore[0].Substring(0, 16).TrimStart('0', '1').TrimStart('0');

                string[] markedCodesByGood = null;
                using (var abt = new DataContextManagementUnit.DataAccess.Contexts.Abt.AbtDbContext())
                {
                    var idGood = abt.RefBarCodes.FirstOrDefault(r => r.BarCode == barCode && r.IsPrimary == false)?.IdGood;

                    if (idGood == null)
                    {
                        idGood = abt.DocEdoPurchasings.FirstOrDefault(d => d.IdDocJournal == idDoc)?
                            .Details?.FirstOrDefault(det => det.BarCode == barCode)?.IdGood;
                    }

                    if (idGood == null)
                        return;

                    markedCodesByGood = abt.DocGoodsDetailsLabels.Where(r => r.IdGood == idGood && r.IdDoc == idDoc).Select(m => m.DmLabel).ToArray();

                    if (markedCodesByGood == null)
                        return;
                }

                var result = markedCodesBefore.Length == markedCodesByGood.Length && markedCodesByGood.All(g => markedCodesBefore.Any(m => m == g));

                var deletedMarkedCodesByProduct = markedCodesBefore.Where(b => !markedCodesAfter.Any(a => a == b)).ToArray();
                result = result && deletedMarkedCodesByProduct.All(p => deletedMarkedCodes.Any(d => d == p));
            }
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

                foreach(var order in OrderManagementStationClient.GetInstance().GetOrdersList().OrderInfos)
                {
                    var statuses = OrderManagementStationClient.GetInstance().GetOrderStatus(order.OrderId);
                    foreach (var product in OrderManagementStationClient.GetInstance().GetProductListFromOrder(order.OrderId))
                    {
                        var blocks = OrderManagementStationClient.GetInstance().GetBlockIdsFromOrder(order.OrderId, product.Key);

                        foreach(var block in blocks.Blocks)
                        {
                            var markedCodes = OrderManagementStationClient.GetInstance().GetMarkedCodesRetry(block.BlockId);
                        }
                    }
                }

                //OrderManagementStationClient.GetInstance().GetBlockIdsFromOrder("7f9600e7-7f05-4029-b039-385c14e36b85", $"0{barCode}");
                //OrderManagementStationClient.GetInstance().GetMarkedCodesFromReport("f0d053cc-91ad-4b33-b549-b6d4bd71d4bc");
                //var order = OrderManagementStationClient.GetInstance().GetOrdersList().OrderInfos.FirstOrDefault(o => o.OrderId == "7f9600e7-7f05-4029-b039-385c14e36b85");
                //var markedCodes = OrderManagementStationClient.GetInstance().GetMarkedCodes("7f9600e7-7f05-4029-b039-385c14e36b85", 1, $"0{barCode}");
                //var markedCodes = OrderManagementStationClient.GetInstance().GetMarkedCodesRetry("e1dae2fb-081d-4ce6-ad66-e3f9d10a208e"/*"beaf72f1-8390-4e7d-9e74-680365ec6679"*/).Codes.ToList();
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
