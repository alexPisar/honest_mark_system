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
            document.Signature = "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";
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

            document.OrganizationEmployeeOrAnotherPerson = new List<object>(new object[] { new Reporter.Entities.AnotherPerson() });
            var organizationRepresentative = new Reporter.Entities.OrganizationRepresentative();
            organizationRepresentative.Position = "Директор";
            organizationRepresentative.OrgName = "ООО \"ВЛАМУР\"";
            organizationRepresentative.Surname = "Мигеркин";
            organizationRepresentative.Name = "Николай";
            organizationRepresentative.Patronymic = "Игоревич";
            ((Reporter.Entities.AnotherPerson)document.OrganizationEmployeeOrAnotherPerson.First()).Item = organizationRepresentative;

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
            xmlDocument.LoadXml(xmlContent);
            xmlDocument.Save($"C:\\Users\\systech\\Desktop\\{document.FileName}.xml");
        }

        [TestMethod]
        public void GetXmlCorrectionRequestTest()
        {
            var document = new ClarificationCorrectionRequestDocument();

            #region Файл
            document.FileName = "DP_UVUTOCH_2BM-5032262632-503201001-201601270943558790381_2BM-2539108495-253901001-201407110842566644066_20220218_da9428c5-3dab-4e58-83a0-416bcba4fe9f";
            document.EdoProgramVersion = "Diadoc 1.0";

            #region УчастЭДО
            document.CreatorEdoId = "2BM-2539108495-253901001-201407110842566644066";
            document.JuridicalInn = "2539108495";
            document.JuridicalKpp = "253901001";
            document.OrgCreatorName = "ООО \"ВЛАМУР\"";
            #endregion

            #region СвУведУточ
            document.ReceiveDate = DateTime.Now;
            document.Text = "Штрихкода испорчены";
            document.ReceivedFileName = "ON_NSCHFDOPPRMARK_2BM-2539108495-253901001-201407110842566644066_2BM-5032262632-503201001-201601270943558790381_20211228_4fbcec52-b5bd-468a-839b-df954b122342";
            document.ReceivedFileSignature = "MIAGCSqGSIb3DQEHAqCAMIACAQExDjAMBggqhQMHAQECAgUAMIAGCSqGSIb3DQEHAQAAoIIKajCCCmYwggoToAMCAQICEQInzXoA3qzPpUe1utOX57PeMAoGCCqFAwcBAQMCMIIB6DEbMBkGCSqGSIb3DQEJARYMY...";
            #endregion

            #region ОтпрДок
            document.SenderEdoId = "2BM-5032262632-503201001-201601270943558790381";
            document.SenderJuridicalInn = "5032262632";
            document.SenderJuridicalKpp = "503201001";
            document.OrgSenderName = "Общество с ограниченной  ответственностью \"Понти Парфюм\"";
            #endregion

            #region Подписант
            document.SignerPosition = "Директор";
            document.SignerSurname = "Мигеркин";
            document.SignerName = "Николай";
            document.SignerPatronymic = "Игоревич";
            #endregion

            #endregion

            var xmlContent = document.GetXmlContent();

            var xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(xmlContent);
            xmlDocument.Save($"C:\\Users\\systech\\Desktop\\{document.FileName}.xml");
        }

        [TestMethod]
        public void CreateSellerXmlTest()
        {
            var document = new UniversalTransferSellerDocument();

            var senderEdoId = "2BM-2504000010-2012052808301120662630000000000";
            var receiverEdoId = "2BM-2539108495-253901001-201407110842566644066";

            #region Файл
            document.EdoProgramVersion = "Вирэй Приходная 1.0.0.0";
            document.FileName = $"ON_NSCHFDOPPRMARK_{receiverEdoId}_{senderEdoId}_{DateTime.Now.ToString("yyyyMMdd")}_{Guid.NewGuid().ToString()}";

            #region СвУчДокОбор

            document.EdoProviderOrgName = "АО \"ПФ \"СКБ Контур\"";
            document.ProviderInn = "6663003127";
            document.EdoId = "2BM";
            document.SenderEdoId = "2BM-2504000010-2012052808301120662630000000000";
            document.ReceiverEdoId = "2BM-2539108495-253901001-201407110842566644066";

            #endregion

            #region Документ

            document.CreateDate = DateTime.Now;
            document.FinSubjectCreator = "Дикарев Вячеслав Юрьевич";
            document.Function = "ДОП";
            document.EconomicLifeDocName = "Документ об отгрузке товаров (выполнении работ), передаче имущественных прав (документ об оказании услуг)";
            document.DocName = "Документ об отгрузке товаров (выполнении работ), передаче имущественных прав (документ об оказании услуг)";

            #region СвСчФакт

            document.DocNumber = "2/23-147192-К-01";
            document.DocDate = DateTime.Now.Date;
            document.CurrencyCode = "643";

            #region СвПрод

            #region ИдСв

            #region СвЮЛУч
            var sellerOrganizationExchangeParticipant = new Reporter.Entities.OrganizationExchangeParticipantEntity();

            sellerOrganizationExchangeParticipant.JuridicalInn = "2504000010";
            sellerOrganizationExchangeParticipant.JuridicalKpp = "253901001";
            sellerOrganizationExchangeParticipant.OrgName = "ООО \"ВИРЭЙ\"";

            document.SellerEntity = sellerOrganizationExchangeParticipant;

            #endregion

            #endregion

            #region Адрес

            #region АдрРФ

            document.SellerAddress = new Reporter.Entities.Address
            {
                CountryCode = "643",
                RussianIndex = "690039",
                RussianRegionCode = "25",
                RussianCity = "Владивосток",
                RussianStreet = "ул Енисейская",
                RussianHouse = "32"
            };

            #endregion

            #endregion

            #endregion

            #region СвПокуп

            #region ИдСв

            #region СвЮЛУч
            var buyerOrganizationExchangeParticipant = new Reporter.Entities.OrganizationExchangeParticipantEntity();

            buyerOrganizationExchangeParticipant.JuridicalInn = "2539108495";
            buyerOrganizationExchangeParticipant.JuridicalKpp = "253901001";
            buyerOrganizationExchangeParticipant.OrgName = "Общество с ограниченной ответственностью \"ВЛАМУР\"";

            document.BuyerEntity = buyerOrganizationExchangeParticipant;


            #endregion

            #endregion

            #region Адрес

            #region АдрРф

            document.BuyerAddress = new Reporter.Entities.Address
            {
                CountryCode = "643",
                RussianIndex = "690039",
                RussianRegionCode = "25",
                RussianCity = "Владивосток",
                RussianStreet = "ул Енисейская",
                RussianHouse = "32"
            };

            #endregion

            #endregion

            #endregion

            #region ДопСвФХЖ1

            document.CurrencyName = "Российский рубль";

            #endregion

            #region ДокПодтвОтгр

            document.DeliveryDocuments = new List<Reporter.Entities.DeliveryDocument>
            {
                new Reporter.Entities.DeliveryDocument
                {
                    DocumentName = "Реализация (акт, накладная, УПД)",
                    DocumentNumber = "п/п 1-8, №2/23-147192-К-01",
                    DocumentDate = DateTime.Now.Date
                }
            };

            #endregion

            #endregion

            #region ТаблСчФакт

            #region СведТов

            document.Products = new List<Reporter.Entities.Product>();
            document.Products.Add(new Reporter.Entities.Product
            {
                Number = 1,
                Description = "Понти Парфюм Парфюмерная вода Merle le Blanc \"Estella Bloom\" (честный знак)",
                UnitCode = "796",
                Quantity = 1,
                Price = 436.50M,
                TaxAmount = 87.30M,
                VatRate = 20,
                OriginCode = "643",
                BarCode = "4623721825408",
                UnitName = "шт",
                OriginCountryName = "Россия",
                MarkedCodes = new List<string> { "010462372182540821D*l0'q3EQVQIi" }
            });

            #endregion

            #endregion

            #region СвПродПер

            document.ContentOperation = "Товары переданы";
            document.ShippingDate = DateTime.Now.Date;
            document.BasisDocumentName = "Без документа-основания";

            #endregion

            #region Подписант

            document.ScopeOfAuthority = Reporter.Enums.SellerScopeOfAuthorityEnum.PersonWhoResponsibleForSigning;
            document.SignerStatus = Reporter.Enums.SellerSignerStatusEnum.EmployeeOfSellerOrganization;

            #region ЮЛ

            document.JuridicalInn = "2504000010";
            document.SignerOrgName = "ООО \"ВИРЭЙ\"";
            document.SignerPosition = "Доверенное лицо";
            document.SignerSurname = "Дикарев";
            document.SignerName = "Вячеслав";
            document.SignerPatronymic = "Юрьевич";

            #endregion

            #endregion

            #endregion
            #endregion

            var xmlContent = document.GetXmlContent();

            var xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(xmlContent);
            xmlDocument.Save($"C:\\Users\\systech\\Desktop\\{document.FileName}.xml");
        }
    }
}
