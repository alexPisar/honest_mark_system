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
        #endregion

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
        public string OrgName { get; set; }

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

        #region Parse Methods
        public override void Parse(string content)
        {
            var xsdDocument = Xml.DeserializeEntity<Файл>(content);

            var contentOperation = xsdDocument?.ИнфПок?.СодФХЖ4?.СвПрин?.КодСодОпер;

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
                    OrgName = jInfo.НаимОрг;
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
