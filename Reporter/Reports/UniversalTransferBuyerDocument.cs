using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UtilitesLibrary.Service;
using Reporter.Enums;
using Reporter.Entities;
using Reporter.XsdClasses.OnNschfdoppok;

namespace Reporter.Reports
{
    public class UniversalTransferBuyerDocument : IReport
    {
        #region Properties
        #region Общая информация

        /// <summary>
        /// Идентификатор файла
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// Версия формата
        /// </summary>
        public const string FormatVersion = "5.01";

        /// <summary>
        /// Версия программы, с помощью которой сформирован файл
        /// </summary>
        public string EdoProgramVersion { get; set; }

        #endregion

        #region Сведения об участниках электронного документооборота

        /// <summary>
        /// Наименование
        /// </summary>
        public string EdoProviderOrgName { get; set; }

        /// <summary>
        /// ИНН
        /// </summary>
        public string Inn { get; set; }

        /// <summary>
        /// Идентификатор оператора электронного документооборота отправителя файла обмена информации покупателя
        /// </summary>
        public string EdoId { get; set; }

        /// <summary>
        /// Идентификатор участника документооборота – отправителя файла обмена информации покупателя
        /// </summary>
        public string SenderId { get; set; }

        /// <summary>
        /// Идентификатор участника документооборота – получателя файла обмена информации покупателя
        /// </summary>
        public string ReceiverId { get; set; }

        #endregion

        #region Документ об отгрузке товаров (выполнении работ), передаче имущественных прав (документ об оказании услуг), включающий в себя счет-фактуру(информация покупателя), или документ об отгрузке товаров(выполнении работ), передаче имущественных прав(документ об оказании услуг) (информация покупателя)

        /// <summary>
        /// Код документа  по КНД
        /// </summary>
        public const string KND = "1115132";

        /// <summary>
        /// Дата и время формирования файла обмена информации покупателя
        /// </summary>
        public DateTime CreateBuyerFileDate { get; set; }

        /// <summary>
        /// Наименование экономического субъекта – составителя файла обмена информации покупателя
        /// </summary>
        public string FinSubjectCreator { get; set; }

        /// <summary>
        /// Основание, по которому экономический субъект является составителем файла обмена информации покупателя
        /// </summary>
        public string ReasonOfCreateFile { get; set; }

        #region Идентификация файла обмена счета-фактуры (информации продавца) или файла обмена информации продавца

        /// <summary>
        /// Идентификатор файла обмена информации продавца
        /// </summary>
        public string SellerFileId { get; set; }

        /// <summary>
        /// Дата и время формирования файла обмена информации продавца
        /// </summary>
        public DateTime CreateSellerFileDate { get; set; }

        /// <summary>
        /// Электронная подпись файла обмена информации продавца
        /// </summary>
        public string Signature { get; set; }

        #endregion

        #region Содержание факта хозяйственной жизни 4 - сведения о принятии товаров (результатов выполненных работ), имущественных прав (о подтверждении факта оказания услуг)

        /// <summary>
        /// Наименование первичного документа, согласованное сторонами сделки 
        /// </summary>
        public string DocName { get; set; }

        /// <summary>
        /// Функция
        /// </summary>
        public string Function { get; set; }

        /// <summary>
        /// Номер счета-фактуры (информации продавца)
        /// </summary>
        public string SellerInvoiceNumber { get; set; }

        /// <summary>
        /// Дата составления (выписки) счета-фактуры (информации продавца)
        /// </summary>
        public DateTime SellerInvoiceDate { get; set; }

        /// <summary>
        /// Вид операции
        /// </summary>
        public string OperationType { get; set; }

        #region Сведения о принятии товаров (результатов выполненных работ), имущественных прав (о подтверждении факта оказания услуг)

        /// <summary>
        /// Содержание операции (текст)
        /// </summary>
        public string ContentOperationText { get; set; }

        /// <summary>
        /// Дата принятия товаров (результатов выполненных работ), имущественных прав (подтверждения факта оказания услуг)
        /// </summary>
        public DateTime DateReceive { get; set; }

        #region Содержание операции
        /// <summary>
        /// Код итога приёмки товара
        /// </summary>
        public AcceptResultEnum AcceptResult { get; set; }

        /// <summary>
        /// Наименование документа, оформляющего расхождения
        /// </summary>
        public string DocumentDiscrepancyName { get; set; }

        /// <summary>
        /// Код вида документа о расхождениях
        /// </summary>
        public DocumentDiscrepancyTypeEnum DocumentDiscrepancyType { get; set; }

        /// <summary>
        /// Номер документа покупателя о расхождениях
        /// </summary>
        public string DocumentDiscrepancyNumber { get; set; }

        /// <summary>
        /// Дата документа о расхождениях
        /// </summary>
        public DateTime DocumentDiscrepancyDate { get; set; }

        /// <summary>
        /// Идентификатор файла обмена документа о расхождениях, сформированного покупателем
        /// </summary>
        public string IdDocumentDiscrepancy { get; set; }
        #endregion

        #region Сведения о лице, принявшем товары (груз)
        /// <summary>
        /// Работник организации покупателя
        /// </summary>
        public OrganizationEmployee OrganizationEmployee { get; set; }

        /// <summary>
        /// Иное лицо
        /// </summary>
        public AnotherPerson AnotherPerson { get; set; }
        #endregion
        #endregion

        #region Информационное поле факта хозяйственной жизни 4

        /// <summary>
        /// Идентификатор  файла информационного поля
        /// </summary>
        public string FileInformationFieldId { get; set; }

        /// <summary>
        /// Идентификатор текстовой информации
        /// </summary>
        public string TextInformationId { get; set; }

        /// <summary>
        /// Значение текстовой информации
        /// </summary>
        public string TextInformation { get; set; }

        #endregion
        #endregion

        #region Подписант
        /// <summary>
        /// Физическое лицо
        /// </summary>
        public IndividualEntity Individual { get; set; }

        /// <summary>
        /// Индивидуальный предприниматель
        /// </summary>
        public JuridicalEntity JuridicalEntity { get; set; }

        /// <summary>
        /// Область полномочий
        /// </summary>
        public ScopeOfAuthorityEnum ScopeOfAuthority { get; set; }

        /// <summary>
        /// Статус
        /// </summary>
        public SignerStatusEnum SignerStatus { get; set; }

        /// <summary>
        /// Основание полномочий (доверия)
        /// </summary>
        public string BasisOfAuthority { get; set; }

        /// <summary>
        /// Основание полномочий (доверия) организации
        /// </summary>
        public string BasisOfAuthorityOrganization { get; set; }

        /// <summary>
        /// ИНН организации
        /// </summary>
        public string JuridicalInn { get; set; }

        /// <summary>
        /// Наименование организации
        /// </summary>
        public string SignerOrgName { get; set; }

        /// <summary>
        /// Должность
        /// </summary>
        public string Position { get; set; }

        /// <summary>
        /// Иные сведения, идентифицирующие физическое лицо
        /// </summary>
        public string OtherInfo { get; set; }

        /// <summary>
        /// Фамилия
        /// </summary>
        public string Surname { get; set; }

        /// <summary>
        /// Имя
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Отчество
        /// </summary>
        public string Patronymic { get; set; }
        #endregion
        #endregion
        #endregion

        #region Parse Methods
        public override void Parse(string content)
        {
            var xsdDocument = Xml.DeserializeEntity<Файл>(content);

            FileName = xsdDocument?.ИдФайл;
            EdoProgramVersion = xsdDocument?.ВерсПрог;

            var infoAboutParticipants = xsdDocument?.СвУчДокОбор;

            if(infoAboutParticipants != null)
            {
                EdoProviderOrgName = infoAboutParticipants?.СвОЭДОтпр?.НаимОрг;
                Inn = infoAboutParticipants?.СвОЭДОтпр?.ИННЮЛ;
                EdoId = infoAboutParticipants?.СвОЭДОтпр?.ИдЭДО;
                SenderId = infoAboutParticipants.ИдОтпр;
                ReceiverId = infoAboutParticipants.ИдПол;
            }

            var buyerInfo = xsdDocument?.ИнфПок;

            if (buyerInfo == null)
                return;

            if(!string.IsNullOrEmpty(buyerInfo.ДатаИнфПок))
                CreateBuyerFileDate = DateTime.ParseExact($"{buyerInfo.ДатаИнфПок} {buyerInfo.ВремИнфПок}", "dd.MM.yyyy hh.mm.ss", System.Globalization.CultureInfo.InvariantCulture);

            FinSubjectCreator = buyerInfo.НаимЭконСубСост;
            ReasonOfCreateFile = buyerInfo.ОснДоверОргСост;

            var sellerInfo = buyerInfo.ИдИнфПрод;

            if(sellerInfo != null)
            {
                SellerFileId = sellerInfo.ИдФайлИнфПр;
                Signature = sellerInfo.ЭП?.FirstOrDefault();

                if (!string.IsNullOrEmpty(sellerInfo.ДатаФайлИнфПр))
                    CreateSellerFileDate = DateTime.ParseExact($"{sellerInfo.ДатаФайлИнфПр} {sellerInfo.ВремФайлИнфПр}", "dd.MM.yyyy hh.mm.ss", System.Globalization.CultureInfo.InvariantCulture);
            }

            var contentOfEconomicLife = buyerInfo?.СодФХЖ4;

            if (contentOfEconomicLife == null)
                return;

            DocName = contentOfEconomicLife.НаимДокОпрПр;
            Function = contentOfEconomicLife.Функция;
            SellerInvoiceNumber = contentOfEconomicLife.НомСчФИнфПр;
            OperationType = contentOfEconomicLife.ВидОперации;

            FileInformationFieldId = contentOfEconomicLife.ИнфПолФХЖ4?.ИдФайлИнфПол;
            TextInformationId = contentOfEconomicLife.ИнфПолФХЖ4?.ТекстИнф?.FirstOrDefault()?.Идентиф;
            TextInformationId = contentOfEconomicLife.ИнфПолФХЖ4?.ТекстИнф?.FirstOrDefault()?.Значен;

            if (!string.IsNullOrEmpty(contentOfEconomicLife.ДатаСчФИнфПр))
                SellerInvoiceDate = DateTime.ParseExact(contentOfEconomicLife.ДатаСчФИнфПр, "dd.MM.yyyy", System.Globalization.CultureInfo.InvariantCulture);

            if (!string.IsNullOrEmpty(contentOfEconomicLife.СвПрин?.СодОпер))
                ContentOperationText = contentOfEconomicLife.СвПрин.СодОпер;

            if (!string.IsNullOrEmpty(contentOfEconomicLife.СвПрин?.ДатаПрин))
                DateReceive = DateTime.ParseExact(contentOfEconomicLife.СвПрин.ДатаПрин, "dd.MM.yyyy", System.Globalization.CultureInfo.InvariantCulture);

            var contentOperation = contentOfEconomicLife.СвПрин?.КодСодОпер;

            if(contentOperation != null)
            {
                if (contentOperation.КодИтога == ФайлИнфПокСодФХЖ4СвПринКодСодОперКодИтога.Item1)
                    AcceptResult = AcceptResultEnum.GoodsAcceptedWithoutDiscrepancy;
                else if (contentOperation.КодИтога == ФайлИнфПокСодФХЖ4СвПринКодСодОперКодИтога.Item2)
                    AcceptResult = AcceptResultEnum.GoodsAcceptedWithDiscrepancy;
                else if (contentOperation.КодИтога == ФайлИнфПокСодФХЖ4СвПринКодСодОперКодИтога.Item3)
                    AcceptResult = AcceptResultEnum.GoodsNotAccepted;

                DocumentDiscrepancyName = contentOperation.НаимДокРасх;

                if (contentOperation.ВидДокРасх == ФайлИнфПокСодФХЖ4СвПринКодСодОперВидДокРасх.Item2)
                    DocumentDiscrepancyType = DocumentDiscrepancyTypeEnum.DocumentWithDiscrepancy;
                else if (contentOperation.ВидДокРасх == ФайлИнфПокСодФХЖ4СвПринКодСодОперВидДокРасх.Item3)
                    DocumentDiscrepancyType = DocumentDiscrepancyTypeEnum.DocumentAboutDiscrepancy;

                DocumentDiscrepancyNumber = contentOperation.НомДокРасх;

                if(!string.IsNullOrEmpty(contentOperation.ДатаДокРасх))
                    DocumentDiscrepancyDate = DateTime.ParseExact(contentOperation.ДатаДокРасх, "dd.MM.yyyy", System.Globalization.CultureInfo.InvariantCulture);

                IdDocumentDiscrepancy = contentOperation.ИдФайлДокРасх;
            }

            var personInfo = contentOfEconomicLife.СвПрин?.СвЛицПрин;

            if(personInfo != null)
            {
                if(personInfo.Item.GetType() == typeof(ФайлИнфПокСодФХЖ4СвПринСвЛицПринРабОргПок))
                {
                    var orgPersonInfo = (ФайлИнфПокСодФХЖ4СвПринСвЛицПринРабОргПок)personInfo.Item;
                    OrganizationEmployee = new OrganizationEmployee();
                    OrganizationEmployee.BasisOfAuthority = orgPersonInfo.ОснПолн;
                    OrganizationEmployee.Position = orgPersonInfo.Должность;
                    OrganizationEmployee.OtherInfo = orgPersonInfo.ИныеСвед;
                    OrganizationEmployee.Surname = orgPersonInfo.ФИО?.Фамилия;
                    OrganizationEmployee.Name = orgPersonInfo.ФИО?.Имя;
                    OrganizationEmployee.Patronymic = orgPersonInfo.ФИО?.Отчество;
                }
                else if (personInfo.Item.GetType() == typeof(ФайлИнфПокСодФХЖ4СвПринСвЛицПринИнЛицо))
                {
                    var otherPerson = (ФайлИнфПокСодФХЖ4СвПринСвЛицПринИнЛицо)personInfo.Item;
                    AnotherPerson = new AnotherPerson();

                    if (otherPerson.Item?.GetType() == typeof(ФайлИнфПокСодФХЖ4СвПринСвЛицПринИнЛицоФЛПрин))
                    {
                        var otherIndividualPerson = (ФайлИнфПокСодФХЖ4СвПринСвЛицПринИнЛицоФЛПрин)otherPerson.Item;
                        AnotherPerson.TrustedIndividual = new TrustedIndividual();
                        AnotherPerson.TrustedIndividual.ReasonOfTrust = otherIndividualPerson.ОснДоверФЛ;
                        AnotherPerson.TrustedIndividual.OtherInfo = otherIndividualPerson.ИныеСвед;
                        AnotherPerson.TrustedIndividual.Surname = otherIndividualPerson?.ФИО?.Фамилия;
                        AnotherPerson.TrustedIndividual.Name = otherIndividualPerson?.ФИО?.Имя;
                        AnotherPerson.TrustedIndividual.Patronymic = otherIndividualPerson?.ФИО?.Отчество;
                    }
                    else if (otherPerson.Item?.GetType() == typeof(ФайлИнфПокСодФХЖ4СвПринСвЛицПринИнЛицоПредОргПрин))
                    {
                        var otherOrgPerson = (ФайлИнфПокСодФХЖ4СвПринСвЛицПринИнЛицоПредОргПрин)otherPerson.Item;
                        AnotherPerson.OrganizationRepresentative = new OrganizationRepresentative();
                        AnotherPerson.OrganizationRepresentative.OrgName = otherOrgPerson.НаимОргПрин;
                        AnotherPerson.OrganizationRepresentative.Position = otherOrgPerson.Должность;
                        AnotherPerson.OrganizationRepresentative.OtherInfo = otherOrgPerson.ИныеСвед;
                        AnotherPerson.OrganizationRepresentative.ReasonOrgTrust = otherOrgPerson.ОснДоверОргПрин;
                        AnotherPerson.OrganizationRepresentative.ReasonTrustPerson = otherOrgPerson.ОснПолнПредПрин;
                        AnotherPerson.OrganizationRepresentative.Surname = otherOrgPerson.ФИО?.Фамилия;
                        AnotherPerson.OrganizationRepresentative.Name = otherOrgPerson.ФИО?.Имя;
                        AnotherPerson.OrganizationRepresentative.Patronymic = otherOrgPerson.ФИО?.Отчество;
                    }
                }
            }

            var signerInfo = xsdDocument?.ИнфПок?.Подписант?.FirstOrDefault();
            if(signerInfo != null)
            {
                if(signerInfo.Item.GetType() == typeof(СвИПТип))
                {
                    var jeInfo = (СвИПТип)signerInfo.Item;
                    JuridicalEntity = new JuridicalEntity();
                    JuridicalEntity.Inn = jeInfo.ИННФЛ;
                    JuridicalEntity.CertificateOfFederalRegistration = jeInfo.СвГосРегИП;
                    JuridicalEntity.OtherInfo = jeInfo.ИныеСвед;
                    JuridicalEntity.Surname = jeInfo.ФИО?.Фамилия;
                    JuridicalEntity.Name = jeInfo.ФИО?.Имя;
                    JuridicalEntity.Patronymic = jeInfo.ФИО?.Отчество;
                }
                else if(signerInfo.Item.GetType() == typeof(СвФЛТип))
                {
                    var indInfo = (СвФЛТип)signerInfo.Item;
                    Individual = new IndividualEntity();
                    Individual.Inn = indInfo.ИННФЛ;
                    Individual.OtherInfo = indInfo.ИныеСвед;
                    Individual.Surname = indInfo.ФИО?.Фамилия;
                    Individual.Name = indInfo.ФИО?.Имя;
                    Individual.Patronymic = indInfo.ФИО?.Отчество;
                }
                else if (signerInfo.Item.GetType() == typeof(ФайлИнфПокПодписантЮЛ))
                {
                    var jInfo = (ФайлИнфПокПодписантЮЛ)signerInfo.Item;
                    JuridicalInn = jInfo.ИННЮЛ;
                    SignerOrgName = jInfo.НаимОрг;
                    OtherInfo = jInfo.ИныеСвед;
                    Position = jInfo.Должн;
                    Surname = jInfo.ФИО?.Фамилия;
                    Name = jInfo.ФИО?.Имя;
                    Patronymic = jInfo.ФИО?.Отчество;
                }

                if (signerInfo.ОблПолн == ФайлИнфПокПодписантОблПолн.Item1)
                    ScopeOfAuthority = ScopeOfAuthorityEnum.PersonWhoMadeOperation;
                else if (signerInfo.ОблПолн == ФайлИнфПокПодписантОблПолн.Item2)
                    ScopeOfAuthority = ScopeOfAuthorityEnum.PersonWhoMadeOperationAndResponsibleForItsExecution;
                else if (signerInfo.ОблПолн == ФайлИнфПокПодписантОблПолн.Item3)
                    ScopeOfAuthority = ScopeOfAuthorityEnum.PersonWhoResponsibleForRegistrationExecution;

                if (signerInfo.Статус == ФайлИнфПокПодписантСтатус.Item3)
                    SignerStatus = SignerStatusEnum.EmployeeOfAnotherAuthorizedOrganization;
                else if (signerInfo.Статус == ФайлИнфПокПодписантСтатус.Item4)
                    SignerStatus = SignerStatusEnum.Individual;
                else if (signerInfo.Статус == ФайлИнфПокПодписантСтатус.Item5)
                    SignerStatus = SignerStatusEnum.EmployeeOfBuyerOrganization;
                else if (signerInfo.Статус == ФайлИнфПокПодписантСтатус.Item6)
                    SignerStatus = SignerStatusEnum.EmployeeOfOrganizationCompilerInformationExchangeFile;

                BasisOfAuthority = signerInfo.ОснПолн;
                BasisOfAuthorityOrganization = signerInfo.ОснПолнОрг;
            }
        }

        public override void Parse(byte[] content)
        {
            var xmlString = Encoding.GetEncoding(1251).GetString(content);
            Parse(xmlString);
        }
        #endregion
    }
}
