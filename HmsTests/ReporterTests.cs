using System;
using System.Xml;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Reporter;
using Reporter.Reports;

namespace HmsTests
{
    [TestClass]
    public class ReporterTests
    {
        [TestMethod]
        public void ParseXmlDocumentTest()
        {
            var reporter = new ReporterDll();

            var xmlDocument = new XmlDocument();
            xmlDocument.Load("C:\\Users\\systech\\Desktop\\Files\\ON_NSCHFDOPPOKMARK_2BM-5032262632-503201001-201601270943558790381_2LT-11000533130_20210924_1967fe1a-ba18-4977-9c7e-0861ddd44298.xml");

            var report = reporter.ParseDocument<UniversalTransferBuyerDocument>(xmlDocument.OuterXml);
        }

        [TestMethod]
        public void GetXmlContentTest()
        {
            var document = new UniversalTransferBuyerDocument();

            #region Файл
            document.FileName = "ON_NSCHFDOPPOKMARK_2BM-5032262632-503201001-201601270943558790381_2LT-11000533130_20210924_1967fe1a-ba18-4977-9c7e-0861ddd44298";
            document.EdoProgramVersion = "EDOLite 1.0";

            #region СвУчДокОбор
            document.EdoProviderOrgName = "АО \"ПФ \"СКБ Контур\"";
            document.ProviderInn = "6663003127";
            document.EdoId = "2BM";
            #region СвОЭДОтпр
            document.ReceiverEdoId = "2BM-5032262632-503201001-201601270943558790381";
            document.SenderEdoId = "2LT-11000533130";
            #endregion
            #endregion

            #region ИнфПок
            document.CreateBuyerFileDate = DateTime.Now;
            document.FinSubjectCreator = "ООО \"ВЛАМУР\", ИНН: 2539108495";

            #region ИдИнфПрод
            document.SellerFileId = "ON_NSCHFDOPPRMARK_2LT-11000533130_2BM-5032262632-503201001-201601270943558790381_20210820_1289f34d-a6d6-449e-a8d8-38ba15518a0c";
            document.CreateSellerFileDate = DateTime.ParseExact($"20.08.2021 11.59.46", "dd.MM.yyyy hh.mm.ss", System.Globalization.CultureInfo.InvariantCulture);
            document.Signature = "MIIP4wYJKoZIhvcNAQcCoIIP1DCCD9ACAQExDDAKBggqhQMHAQECAjALBgkqhkiG9w0BBwGgggpqMIIKZjCCChOgAwIBAgIRAifNegDerM+lR7W605fns94wCgYIKoUDBwEBAwIwggHoMRswGQYJKoZIhvcNAQkBFgxjYUBzZXJ0dW0ucnUxGDAWBgUqhQNkARINMTExNjY3MzAwODUzOTEaMBgGCCqFAwOBAwEBEgwwMDY2NzMyNDAzMjgxCzAJBgNVBAYTAlJVMTMwMQYDVQQIDCo2NiDQodCy0LXRgNC00LvQvtCy0YHQutCw0Y8g0L7QsdC70LDRgdGC0YwxITAfBgNVBAcMGNCV0LrQsNGC0LXRgNC40L3QsdGD0YDQszFSMFAGA1UECQxJ0YPQu9C40YbQsCDQo9C70YzRj9C90L7QstGB0LrQsNGPLCDQtC4gMTMsINC70LjRgtC10YAg0JAsINC+0YTQuNGBIDIwOSDQkTFsMGoGA1UECgxj0J7QsdGJ0LXRgdGC0LLQviDRgSDQvtCz0YDQsNC90LjRh9C10L3QvdC+0Lkg0L7RgtCy0LXRgtGB0YLQstC10L3QvdC+0YHRgtGM0Y4gItCh0LXRgNGC0YPQvC3Qn9GA0L4iMWwwagYDVQQDDGPQntCx0YnQtdGB0YLQstC+INGBINC+0LPRgNCw0L3QuNGH0LXQvdC90L7QuSDQvtGC0LLQtdGC0YHRgtCy0LXQvdC90L7RgdGC0YzRjiAi0KHQtdGA0YLRg9C8LdCf0YDQviIwHhcNMjEwMzAxMDcyMjA3WhcNMjIwMzAxMDcyNTQ1WjCCAcoxIDAeBgkqhkiG9w0BCQEWEXAxQHBvbnRpcGFyZnVtLnJ1MRowGAYIKoUDA4EDAQESDDAwNTAzMjI2MjYzMjEWMBQGBSqFA2QDEgsxODMyNDgwNDg3MzEYMBYGBSqFA2QBEg0xMTM1MDMyMDAxMjY4MRswGQYDVQQMDBLQkdGD0YXQs9Cw0LvRgtC10YAxKTAnBgNVBAoMINCe0J7QniAi0J/QntCd0KLQmCDQn9CQ0KDQpNCu0JwiMVYwVAYDVQQJDE3Qo9CbIDMt0K8g0KXQntCg0J7QqNCB0JLQodCa0JDQrywg0JTQntCcIDIsINCh0KLQoCAxLCDQrdCiLiA2INCf0J7QnNCV0KkuIDHQkTEVMBMGA1UEBwwM0JzQvtGB0LrQstCwMRwwGgYDVQQIDBM3NyDQsy4g0JzQvtGB0LrQstCwMQswCQYDVQQGEwJSVTEsMCoGA1UEKgwj0KLQsNGC0YzRj9C90LAg0J3QuNC60L7Qu9Cw0LXQstC90LAxHTAbBgNVBAQMFNCf0LDRgNGF0L7QvNC10L3QutC+MSkwJwYDVQQDDCDQntCe0J4gItCf0J7QndCi0Jgg0J/QkNCg0KTQrtCcIjBmMB8GCCqFAwcBAQEBMBMGByqFAwICJAAGCCqFAwcBAQICA0MABECcOmRbbf5eNGei0DuM58524tPvsoGrDan3w6OYmPXYqQNAHbASmEJrDXiZbjDupbBQULZ6jN2ckaogw8G3KcTlo4IFqTCCBaUwDgYDVR0PAQH/BAQDAgTwMBwGA1UdEQQVMBOBEXAxQHBvbnRpcGFyZnVtLnJ1MBMGA1UdIAQMMAowCAYGKoUDZHEBMEIGA1UdJQQ7MDkGCCsGAQUFBwMCBgcqhQMCAiIGBggrBgEFBQcDBAYHKoUDA4E5AQYIKoUDAwUKAgwGByqFAwMHCAEwggEQBggrBgEFBQcBAQSCAQIwgf8wNwYIKwYBBQUHMAGGK2h0dHA6Ly9wa2kuc2VydHVtLXByby5ydS9vY3NwcTIwMTIvb2NzcC5zcmYwOAYIKwYBBQUHMAGGLGh0dHA6Ly9wa2kyLnNlcnR1bS1wcm8ucnUvb2NzcHEyMDEyL29jc3Auc3JmMEYGCCsGAQUFBzAChjpodHRwOi8vY2Euc2VydHVtLXByby5ydS9jZXJ0aWZpY2F0ZXMvc2VydHVtLXByby1xLTIwMjAuY3J0MEIGCCsGAQUFBzAChjZodHRwOi8vY2Euc2VydHVtLnJ1L2NlcnRpZmljYXRlcy9zZXJ0dW0tcHJvLXEtMjAyMC5jcnQwKwYDVR0QBCQwIoAPMjAyMTAzMDEwNzIyMDZagQ8yMDIyMDMwMTA3MjU0NVowggEzBgUqhQNkcASCASgwggEkDCsi0JrRgNC40L/RgtC+0J/RgNC+IENTUCIgKNCy0LXRgNGB0LjRjyA0LjApDFMi0KPQtNC+0YHRgtC+0LLQtdGA0Y/RjtGJ0LjQuSDRhtC10L3RgtGAICLQmtGA0LjQv9GC0L7Qn9GA0L4g0KPQpiIg0LLQtdGA0YHQuNC4IDIuMAxP0KHQtdGA0YLQuNGE0LjQutCw0YIg0YHQvtC+0YLQstC10YLRgdGC0LLQuNGPIOKEliDQodCkLzEyNC0zOTY2INC+0YIgMTUuMDEuMjAyMQxP0KHQtdGA0YLQuNGE0LjQutCw0YIg0YHQvtC+0YLQstC10YLRgdGC0LLQuNGPIOKEliDQodCkLzEyOC0zNTkyINC+0YIgMTcuMTAuMjAxODAjBgUqhQNkbwQaDBgi0JrRgNC40L/RgtC+0J/RgNC+IENTUCIwdwYDVR0fBHAwbjA3oDWgM4YxaHR0cDovL2NhLnNlcnR1bS1wcm8ucnUvY2RwL3NlcnR1bS1wcm8tcS0yMDIwLmNybDAzoDGgL4YtaHR0cDovL2NhLnNlcnR1bS5ydS9jZHAvc2VydHVtLXByby1xLTIwMjAuY3JsMIGCBgcqhQMCAjECBHcwdTBlFkBodHRwczovL2NhLmtvbnR1ci5ydS9hYm91dC9kb2N1bWVudHMvY3J5cHRvcHJvLWxpY2Vuc2UtcXVhbGlmaWVkDB3QodCa0JEg0JrQvtC90YLRg9GAINC4INCU0JfQngMCBeAEDBQ/KwBmv3Hg94Tb0DCCAWAGA1UdIwSCAVcwggFTgBQ7G3dFLcZyfqJyNnHGWKo3GtJ9baGCASykggEoMIIBJDEeMBwGCSqGSIb3DQEJARYPZGl0QG1pbnN2eWF6LnJ1MQswCQYDVQQGEwJSVTEYMBYGA1UECAwPNzcg0JzQvtGB0LrQstCwMRkwFwYDVQQHDBDQsy4g0JzQvtGB0LrQstCwMS4wLAYDVQQJDCXRg9C70LjRhtCwINCi0LLQtdGA0YHQutCw0Y8sINC00L7QvCA3MSwwKgYDVQQKDCPQnNC40L3QutC+0LzRgdCy0Y/Qt9GMINCg0L7RgdGB0LjQuDEYMBYGBSqFA2QBEg0xMDQ3NzAyMDI2NzAxMRowGAYIKoUDA4EDAQESDDAwNzcxMDQ3NDM3NTEsMCoGA1UEAwwj0JzQuNC90LrQvtC80YHQstGP0LfRjCDQoNC+0YHRgdC40LiCCwD3zYtIAAAAAAQnMB0GA1UdDgQWBBSjotd5Ba4lKBmBFsn9xouHHoyGYDAKBggqhQMHAQEDAgNBAK6oLBMBsDOU6ZPgE3AnZDfR5J6jOVTgq303wLGweyJk4QtUsiDk+75NBkEBtvoNA8/fQEgDE1VvjO30AilpsUYxggVAMIIFPAIBATCCAf8wggHoMRswGQYJKoZIhvcNAQkBFgxjYUBzZXJ0dW0ucnUxGDAWBgUqhQNkARINMTExNjY3MzAwODUzOTEaMBgGCCqFAwOBAwEBEgwwMDY2NzMyNDAzMjgxCzAJBgNVBAYTAlJVMTMwMQYDVQQIDCo2NiDQodCy0LXRgNC00LvQvtCy0YHQutCw0Y8g0L7QsdC70LDRgdGC0YwxITAfBgNVBAcMGNCV0LrQsNGC0LXRgNC40L3QsdGD0YDQszFSMFAGA1UECQxJ0YPQu9C40YbQsCDQo9C70YzRj9C90L7QstGB0LrQsNGPLCDQtC4gMTMsINC70LjRgtC10YAg0JAsINC+0YTQuNGBIDIwOSDQkTFsMGoGA1UECgxj0J7QsdGJ0LXRgdGC0LLQviDRgSDQvtCz0YDQsNC90LjRh9C10L3QvdC+0Lkg0L7RgtCy0LXRgtGB0YLQstC10L3QvdC+0YHRgtGM0Y4gItCh0LXRgNGC0YPQvC3Qn9GA0L4iMWwwagYDVQQDDGPQntCx0YnQtdGB0YLQstC+INGBINC+0LPRgNCw0L3QuNGH0LXQvdC90L7QuSDQvtGC0LLQtdGC0YHRgtCy0LXQvdC90L7RgdGC0YzRjiAi0KHQtdGA0YLRg9C8LdCf0YDQviICEQInzXoA3qzPpUe1utOX57PeMAoGCCqFAwcBAQICoIICwzAYBgkqhkiG9w0BCQMxCwYJKoZIhvcNAQcBMBwGCSqGSIb3DQEJBTEPFw0yMTA4MjAwOTEwMjVaMC8GCSqGSIb3DQEJBDEiBCC8xUgViGPGgaH7tk24GVLSK5itleMg5Yv3VKS9ZM/3rjCCAlYGCyqGSIb3DQEJEAIvMYICRTCCAkEwggI9MIICOTAKBggqhQMHAQECAgQgHg0M1weQIQpLzJ3N9OSBgzpgQQVOLqAKy1kWEauWPeowggIHMIIB8KSCAewwggHoMRswGQYJKoZIhvcNAQkBFgxjYUBzZXJ0dW0ucnUxGDAWBgUqhQNkARINMTExNjY3MzAwODUzOTEaMBgGCCqFAwOBAwEBEgwwMDY2NzMyNDAzMjgxCzAJBgNVBAYTAlJVMTMwMQYDVQQIDCo2NiDQodCy0LXRgNC00LvQvtCy0YHQutCw0Y8g0L7QsdC70LDRgdGC0YwxITAfBgNVBAcMGNCV0LrQsNGC0LXRgNC40L3QsdGD0YDQszFSMFAGA1UECQxJ0YPQu9C40YbQsCDQo9C70YzRj9C90L7QstGB0LrQsNGPLCDQtC4gMTMsINC70LjRgtC10YAg0JAsINC+0YTQuNGBIDIwOSDQkTFsMGoGA1UECgxj0J7QsdGJ0LXRgdGC0LLQviDRgSDQvtCz0YDQsNC90LjRh9C10L3QvdC+0Lkg0L7RgtCy0LXRgtGB0YLQstC10L3QvdC+0YHRgtGM0Y4gItCh0LXRgNGC0YPQvC3Qn9GA0L4iMWwwagYDVQQDDGPQntCx0YnQtdGB0YLQstC+INGBINC+0LPRgNCw0L3QuNGH0LXQvdC90L7QuSDQvtGC0LLQtdGC0YHRgtCy0LXQvdC90L7RgdGC0YzRjiAi0KHQtdGA0YLRg9C8LdCf0YDQviICEQInzXoA3qzPpUe1utOX57PeMB8GCCqFAwcBAQEBMBMGByqFAwICJAAGCCqFAwcBAQICBEDeI02K+6OyJLuCiEbf2jOeUGG5HsOfE2Nee8OahbGTgg9nHp96IhJhIysEyeTNC8YV28IHsVwh0eq+m/gZWoea";
            #endregion

            #region СодФХЖ4
            document.DocName = "Счет-фактура и документ об отгрузке товаров (выполнении работ), передаче имущественных прав (документ об оказании услуг)";
            document.Function = "СЧФДОП";
            document.SellerInvoiceNumber = "1033";
            document.SellerInvoiceDate = DateTime.ParseExact($"20.08.2021", "dd.MM.yyyy", System.Globalization.CultureInfo.InvariantCulture);

            #region СвПрин
            document.DateReceive = DateTime.ParseExact($"24.09.2021", "dd.MM.yyyy", System.Globalization.CultureInfo.InvariantCulture);
            #region КодСодОпер
            document.AcceptResult = Reporter.Enums.AcceptResultEnum.GoodsAcceptedWithoutDiscrepancy;
            #endregion

            #region СвЛицПрин
            #region ИнЛицо
            #region ПредОргПрин

            document.AnotherPerson = new Reporter.Entities.AnotherPerson();
            document.AnotherPerson.OrganizationRepresentative = new Reporter.Entities.OrganizationRepresentative();
            document.AnotherPerson.OrganizationRepresentative.Position = "Директор";
            document.AnotherPerson.OrganizationRepresentative.OrgName = "ООО \"ВЛАМУР\"";
            document.AnotherPerson.OrganizationRepresentative.Surname = "Мигеркин";
            document.AnotherPerson.OrganizationRepresentative.Name = "Николай";
            document.AnotherPerson.OrganizationRepresentative.Patronymic = "Игоревич";

            #endregion
            #endregion
            #endregion
            #endregion
            #endregion

            #region Подписант
            document.ScopeOfAuthority = Reporter.Enums.ScopeOfAuthorityEnum.PersonWhoResponsibleForRegistrationExecution;
            document.SignerStatus = Reporter.Enums.SignerStatusEnum.Individual;
            document.BasisOfAuthority = "Должностные обязанности";
            #region ЮЛ
            document.JuridicalInn = "2539108495";
            document.SignerPosition = "Директор";
            document.SignerSurname = "Мигеркин";
            document.SignerName = "Николай";
            document.SignerPatronymic = "Игоревич";
            #endregion
            #endregion
            #endregion
            #endregion

            var reporter = new ReporterDll();
            var xmlContent = document.GetXmlContent();

            var xmlDocument = new XmlDocument();
            xmlDocument.LoadXml($"<?xml version=\"1.0\" encoding=\"windows-1251\"?>{xmlContent}");
            xmlDocument.Save($"C:\\Users\\systech\\Desktop\\{document.FileName}.xml");
        }
    }
}
