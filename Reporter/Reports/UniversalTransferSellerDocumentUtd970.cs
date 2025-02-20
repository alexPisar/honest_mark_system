using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UtilitesLibrary.Service;
using Reporter.Entities;
using Reporter.Enums;
using Reporter.XsdClasses.OnNschfdopprUtd970;

namespace Reporter.Reports
{
    public class UniversalTransferSellerDocumentUtd970 : IReport
    {
        private const string codeOfRussia = "643";

        #region Properties
        #region Общая информация

        /// <summary>
        /// Идентификатор файла
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// Версия формата
        /// </summary>
        public const string FormatVersion = "5.03";

        /// <summary>
        /// Версия программы, с помощью которой сформирован файл
        /// </summary>
        public string EdoProgramVersion { get; set; }

        #endregion

        #region Сведения об участниках электронного документооборота

        /// <summary>
        /// Идентификатор оператора электронного документооборота отправителя файла обмена информации покупателя
        /// </summary>
        public string EdoId { get; set; }

        /// <summary>
        /// Идентификатор участника документооборота – отправителя файла обмена информации покупателя
        /// </summary>
        public string SenderEdoId { get; set; }

        /// <summary>
        /// Идентификатор участника документооборота – получателя файла обмена информации покупателя
        /// </summary>
        public string ReceiverEdoId { get; set; }

        #endregion

        #region Документ
        /// <summary>
        /// Код документа  по КНД
        /// </summary>
        public const string KND = "1115131";

        /// <summary>
        /// Дата и время формирования файла обмена счета-фактуры (информации продавца)
        /// </summary>
        public DateTime CreateDate { get; set; }

        /// <summary>
        /// Функция
        /// </summary>
        public string Function { get; set; }

        /// <summary>
        /// Наименование первичного документа, определенное организацией (согласованное сторонами сделки)
        /// </summary>
        public string DocName { get; set; }

        /// <summary>
        /// Порядковый номер счета-фактуры (строка 1 счета-фактуры), документа об отгрузке товаров (выполнении работ), передаче имущественных прав (документа об оказании услуг)
        /// </summary>
        public string DocNumber { get; set; }

        /// <summary>
        /// Наименование документа по факту хозяйственной жизни
        /// </summary>
        public string EconomicLifeDocName { get; set; }

        /// <summary>
        /// Наименование экономического субъекта – составителя файла обмена счета-фактуры (информации продавца)
        /// </summary>
        public string FinSubjectCreator { get; set; }

        /// <summary>
        /// Код из Общероссийского классификатора валют
        /// </summary>
        public string CurrencyCode { get; set; }

        /// <summary>
        /// Дата составления (выписки) счета-фактуры (строка 1 счета-фактуры), документа об отгрузке товаров (выполнении работ), передаче имущественных прав (документа об оказании услуг)
        /// </summary>
        public DateTime DocDate { get; set; }

        #region Сведения о продавце (строки 2, 2а, 2б счета-фактуры)

        public object SellerEntity { get; set; }
        public Address SellerAddress { get; set; }

        #endregion

        #region Сведения о покупателе (строки 6, 6а, 6б счета-фактуры)

        public object BuyerEntity { get; set; }
        public Address BuyerAddress { get; set; }

        #endregion

        #region Дополнительные сведения об участниках факта хозяйственной жизни, основаниях и обстоятельствах его проведения

        /// <summary>
        /// Наименование валюты
        /// </summary>
        public string CurrencyName { get; set; }

        #endregion

        #region Реквизиты документа, подтверждающего отгрузку товаров (работ, услуг, имущественных прав)

        public List<DeliveryDocument> DeliveryDocuments { get; set; } = new List<DeliveryDocument>();

        #endregion
        #endregion

        #region Сведения таблицы счета-фактуры (содержание факта хозяйственной жизни 2 - наименование и другая информация об отгруженных товарах (выполненных работах, оказанных услугах), о переданных имущественных правах
        #region Сведения о товарах
        public List<Product> Products { get; set; }
        #endregion

        #region Всего к оплате

        #endregion
        #endregion

        #region Содержание факта хозяйственной жизни 3 – сведения о факте отгрузки товаров (выполнения работ), передачи имущественных прав (о предъявлении оказанных услуг)
        #region Сведения о передаче (сдаче) товаров (результатов работ), имущественных прав (о предъявлении оказанных услуг)

        /// <summary>
        /// Содержание операции
        /// </summary>
        public string ContentOperation { get; set; }

        /// <summary>
        /// Вид операции
        /// </summary>
        public string OperationType { get; set; }

        /// <summary>
        /// Дата отгрузки товаров (передачи результатов работ), передачи имущественных прав (предъявления оказанных услуг)
        /// </summary>
        public DateTime? ShippingDate { get; set; } = null;

        #region Основание отгрузки товаров (передачи результатов работ), передачи  имущественных прав (предъявления оказанных услуг)

        /// <summary>
        /// Наименование документа - основания
        /// </summary>
        public string BasisDocumentName { get; set; }

        /// <summary>
        /// Номер документа - основания
        /// </summary>
        public string BasisDocumentNumber { get; set; }

        /// <summary>
        /// Дата документа - основания
        /// </summary>
        public DateTime? BasisDocumentDate { get; set; } = null;

        /// <summary>
        /// Дополнительные сведения
        /// </summary>
        public string BasisDocumentOtherInfo { get; set; }

        /// <summary>
        /// Идентификатор документа - основания
        /// </summary>
        public string BasisDocumentId { get; set; }

        #endregion

        #endregion
        #endregion

        #region Сведения о лице, подписывающем файл обмена счета-фактуры (информации продавца) в электронной форме

        /// <summary>
        /// Тип подписи
        /// </summary>
        public SignTypeEnum SignType { get; set; }

        /// <summary>
        /// Способ подтверждения полномочий представителя на подписание документа
        /// </summary>
        public MethodOfConfirmingAuthorityEnum MethodOfConfirmingAuthority { get; set; }

        /// <summary>
        /// Сведения о доверенности в электронной форме в машиночитаемом виде, используемой для подтверждения полномочий представителя
        /// </summary>
        public ElectronicPowerOfAttorney ElectronicPowerOfAttorney { get; set; }

        /// <summary>
        /// ИНН организации
        /// </summary>
        public string JuridicalInn { get; set; }

        /// <summary>
        /// Дата подписи
        /// </summary>
        public DateTime? SignDate { get; set; }

        /// <summary>
        /// Должность
        /// </summary>
        public string SignerPosition { get; set; }

        /// <summary>
        /// Фамилия
        /// </summary>
        public string SignerSurname { get; set; }

        /// <summary>
        /// Имя
        /// </summary>
        public string SignerName { get; set; }

        /// <summary>
        /// Отчество
        /// </summary>
        public string SignerPatronymic { get; set; }

        #endregion
        #endregion

        #region Parse Methods
        public void Parse(byte[] content)
        {
            var xmlString = Encoding.GetEncoding(1251).GetString(content);
            Parse(xmlString);
        }

        public void Parse(string content)
        {
            var xsdDocument = Xml.DeserializeEntity<Файл>(content);

            FileName = xsdDocument?.ИдФайл;
            EdoProgramVersion = xsdDocument?.ВерсПрог;

            var document = xsdDocument.Документ;
            if (document != null)
            {
                if (!(string.IsNullOrEmpty(document.ДатаИнфПр) || string.IsNullOrEmpty(document.ВремИнфПр)))
                    CreateDate = DateTime.ParseExact($"{document.ДатаИнфПр} {document.ВремИнфПр}", "dd.MM.yyyy HH.mm.ss", System.Globalization.CultureInfo.InvariantCulture);

                if (document.Функция == ФайлДокументФункция.СЧФ)
                    Function = "СЧФ";
                else if (document.Функция == ФайлДокументФункция.СЧФДОП)
                    Function = "СЧФДОП";
                else if (document.Функция == ФайлДокументФункция.ДОП)
                    Function = "ДОП";
                else if (document.Функция == ФайлДокументФункция.СвРК)
                    Function = "СвРК";
                else if (document.Функция == ФайлДокументФункция.СвЗК)
                    Function = "СвЗК";

                DocName = document.НаимДокОпр;
                DocNumber = document.СвСчФакт?.НомерДок;

                if (!string.IsNullOrEmpty(document.СвСчФакт?.ДатаДок))
                    DocDate = DateTime.ParseExact(document.СвСчФакт.ДатаДок, "dd.MM.yyyy", System.Globalization.CultureInfo.InvariantCulture);

                var goods = document.ТаблСчФакт?.СведТов ?? new ФайлДокументТаблСчФактСведТов[] { };
                Products = new List<Product>();

                foreach (var good in goods)
                {
                    var product = new Product()
                    {
                        Description = good.НаимТов,
                        Quantity = good.КолТов,
                        Price = good.ЦенаТов,
                        TaxAmount = good.СумНал?.Item as decimal?,
                        Subtotal = good.СтТовУчНал,
                        Number = Convert.ToInt32(good.НомСтр)
                    };

                    if (good?.ИнфПолФХЖ2 != null && (good?.ИнфПолФХЖ2?.Count() ?? 0) > 0)
                    {
                        var barCodeField = good.ИнфПолФХЖ2.FirstOrDefault(i => i.Идентиф == "Штрихкод");

                        if (barCodeField == null)
                            barCodeField = good.ИнфПолФХЖ2.FirstOrDefault(i => i.Идентиф == "штрихкод");

                        if (barCodeField != null)
                            product.BarCode = barCodeField.Значен;
                    }

                    if (!string.IsNullOrEmpty(good?.ДопСведТов?.КодТов))
                    {
                        if (string.IsNullOrEmpty(product.BarCode))
                        {
                            if (good.ДопСведТов.КодТов.StartsWith("0") && good.ДопСведТов.КодТов.Length == 14)
                                product.BarCode = good.ДопСведТов.КодТов.TrimStart('0');
                        }
                    }

                    product.MarkedCodes = new List<string>();
                    product.TransportPackingIdentificationCode = new List<string>();

                    if (good?.ДопСведТов?.НомСредИдентТов != null)
                    {
                        foreach (var code in good.ДопСведТов.НомСредИдентТов)
                        {
                            if (!string.IsNullOrEmpty(code.ИдентТрансУпак))
                                product.TransportPackingIdentificationCode.Add(code.ИдентТрансУпак);

                            if (code.Items != null)
                            {
                                product.MarkedCodes.AddRange(code.Items);

                                string markedCodeExample = code.Items?.FirstOrDefault();
                                if (string.IsNullOrEmpty(product.BarCode) && !string.IsNullOrEmpty(markedCodeExample))
                                    if (markedCodeExample.Length == 31)
                                        product.BarCode = markedCodeExample.Substring(0, 16).TrimStart('0', '1').TrimStart('0');
                            }
                        }
                    }

                    if (string.IsNullOrEmpty(product.BarCode))
                        product.BarCode = good?.ДопСведТов?.КодТов;

                    Products.Add(product);
                }
            }
        }
        #endregion

        #region GetXmlContent
        public string GetXmlContent()
        {
            var xsdDocument = new Файл();

            xsdDocument.ИдФайл = FileName;
            xsdDocument.ВерсПрог = EdoProgramVersion;

            xsdDocument.Документ = new ФайлДокумент();
            xsdDocument.Документ.ДатаИнфПр = CreateDate.ToString("dd.MM.yyyy");
            xsdDocument.Документ.ВремИнфПр = CreateDate.ToString("HH.mm.ss");

            if (Function == "СЧФ")
                xsdDocument.Документ.Функция = ФайлДокументФункция.СЧФ;
            else if (Function == "СЧФДОП")
                xsdDocument.Документ.Функция = ФайлДокументФункция.СЧФДОП;
            else if (Function == "ДОП")
                xsdDocument.Документ.Функция = ФайлДокументФункция.ДОП;
            else if (Function == "СвРК")
                xsdDocument.Документ.Функция = ФайлДокументФункция.СвРК;
            else if (Function == "СвЗК")
                xsdDocument.Документ.Функция = ФайлДокументФункция.СвЗК;

            xsdDocument.Документ.НаимДокОпр = DocName;
            xsdDocument.Документ.ПоФактХЖ = EconomicLifeDocName;
            xsdDocument.Документ.НаимЭконСубСост = FinSubjectCreator;

            xsdDocument.Документ.СвСчФакт = new ФайлДокументСвСчФакт();
            xsdDocument.Документ.СвСчФакт.НомерДок = DocNumber;
            xsdDocument.Документ.СвСчФакт.ДатаДок = DocDate.ToString("dd.MM.yyyy");

            xsdDocument.Документ.СвСчФакт.СвПрод = new УчастникТип[1];
            xsdDocument.Документ.СвСчФакт.СвПрод[0] = new УчастникТип();

            if (SellerEntity != null)
            {
                xsdDocument.Документ.СвСчФакт.СвПрод[0].ИдСв = new УчастникТипИдСв();

                if (SellerEntity as OrganizationExchangeParticipantEntity != null)
                {
                    var organizationExchangeParticipant = SellerEntity as OrganizationExchangeParticipantEntity;

                    var item = new УчастникТипИдСвСвЮЛУч();
                    item.ИННЮЛ = organizationExchangeParticipant.JuridicalInn;
                    item.КПП = organizationExchangeParticipant.JuridicalKpp;
                    item.НаимОрг = organizationExchangeParticipant.OrgName;

                    xsdDocument.Документ.СвСчФакт.СвПрод[0].ИдСв.Item = item;
                }
                else if (SellerEntity as IndividualEntity != null)
                {
                    var individualEntity = SellerEntity as IndividualEntity;

                    var item = new УчастникТипИдСвСвФЛУч();
                    item.ИННФЛ = individualEntity.Inn;
                    item.ФИО = new ФИОТип
                    {
                        Фамилия = individualEntity.Surname,
                        Имя = individualEntity.Name,
                        Отчество = individualEntity.Patronymic
                    };
                    item.ИныеСвед = individualEntity.OtherInfo;
                    xsdDocument.Документ.СвСчФакт.СвПрод[0].ИдСв.Item = item;
                }
                else if (SellerEntity as JuridicalEntity != null)
                {
                    var juridicalEntity = SellerEntity as JuridicalEntity;

                    var item = new УчастникТипИдСвСвИП();
                    item.ИННФЛ = juridicalEntity.Inn;
                    item.ИныеСвед = juridicalEntity.OtherInfo;
                    item.СвГосРегИП = juridicalEntity.CertificateOfFederalRegistration;
                    item.ФИО = new ФИОТип
                    {
                        Фамилия = juridicalEntity.Surname,
                        Имя = juridicalEntity.Name,
                        Отчество = juridicalEntity.Patronymic
                    };
                    xsdDocument.Документ.СвСчФакт.СвПрод[0].ИдСв.Item = item;
                }
            }

            xsdDocument.Документ.СвСчФакт.СвПрод[0].Адрес = new АдресТип();
            if (SellerAddress?.CountryCode == codeOfRussia)
            {
                var russianAddress = new АдрРФТип();
                russianAddress.КодРегион = SellerAddress.RussianRegionCode;
                russianAddress.НаимРегион = SellerAddress.RussianRegionName;

                if (!string.IsNullOrEmpty(SellerAddress.RussianCity))
                    russianAddress.Город = SellerAddress.RussianCity;

                if (!string.IsNullOrEmpty(SellerAddress.RussianIndex))
                    russianAddress.Индекс = SellerAddress.RussianIndex;

                if (!string.IsNullOrEmpty(SellerAddress.RussianStreet))
                    russianAddress.Улица = SellerAddress.RussianStreet;

                if (!string.IsNullOrEmpty(SellerAddress.RussianHouse))
                    russianAddress.Дом = SellerAddress.RussianHouse;

                if (!string.IsNullOrEmpty(SellerAddress.RussianFlat))
                    russianAddress.Кварт = SellerAddress.RussianFlat;

                xsdDocument.Документ.СвСчФакт.СвПрод[0].Адрес.Item = russianAddress;
            }
            else if (SellerAddress != null)
            {
                var foreignAddress = new АдрИнфТип();
                foreignAddress.КодСтр = SellerAddress.CountryCode;
                foreignAddress.НаимСтран = SellerAddress.CountryName;
                foreignAddress.АдрТекст = SellerAddress.ForeignTextAddress;

                xsdDocument.Документ.СвСчФакт.СвПрод[0].Адрес.Item = foreignAddress;
            }

            xsdDocument.Документ.СвСчФакт.СвПокуп = new УчастникТип[1];
            xsdDocument.Документ.СвСчФакт.СвПокуп[0] = new УчастникТип();

            if (BuyerEntity != null)
            {
                xsdDocument.Документ.СвСчФакт.СвПокуп[0].ИдСв = new УчастникТипИдСв();

                if (BuyerEntity as OrganizationExchangeParticipantEntity != null)
                {
                    var organizationExchangeParticipant = BuyerEntity as OrganizationExchangeParticipantEntity;

                    var item = new УчастникТипИдСвСвЮЛУч();
                    item.ИННЮЛ = organizationExchangeParticipant.JuridicalInn;
                    item.КПП = organizationExchangeParticipant.JuridicalKpp;
                    item.НаимОрг = organizationExchangeParticipant.OrgName;

                    xsdDocument.Документ.СвСчФакт.СвПокуп[0].ИдСв.Item = item;
                }
                else if (BuyerEntity as IndividualEntity != null)
                {
                    var individualEntity = BuyerEntity as IndividualEntity;

                    var item = new УчастникТипИдСвСвФЛУч();
                    item.ИННФЛ = individualEntity.Inn;
                    item.ФИО = new ФИОТип
                    {
                        Фамилия = individualEntity.Surname,
                        Имя = individualEntity.Name,
                        Отчество = individualEntity.Patronymic
                    };
                    item.ИныеСвед = individualEntity.OtherInfo;
                    xsdDocument.Документ.СвСчФакт.СвПокуп[0].ИдСв.Item = item;
                }
                else if (BuyerEntity as JuridicalEntity != null)
                {
                    var juridicalEntity = BuyerEntity as JuridicalEntity;

                    var item = new УчастникТипИдСвСвИП();
                    item.ИННФЛ = juridicalEntity.Inn;
                    item.ИныеСвед = juridicalEntity.OtherInfo;
                    item.СвГосРегИП = juridicalEntity.CertificateOfFederalRegistration;
                    item.ФИО = new ФИОТип
                    {
                        Фамилия = juridicalEntity.Surname,
                        Имя = juridicalEntity.Name,
                        Отчество = juridicalEntity.Patronymic
                    };
                    xsdDocument.Документ.СвСчФакт.СвПокуп[0].ИдСв.Item = item;
                }
            }

            xsdDocument.Документ.СвСчФакт.СвПокуп[0].Адрес = new АдресТип();
            if (BuyerAddress?.CountryCode == codeOfRussia)
            {
                var russianAddress = new АдрРФТип();
                russianAddress.КодРегион = BuyerAddress.RussianRegionCode;
                russianAddress.НаимРегион = BuyerAddress.RussianRegionName;

                if (!string.IsNullOrEmpty(BuyerAddress.RussianCity))
                    russianAddress.Город = BuyerAddress.RussianCity;

                if (!string.IsNullOrEmpty(BuyerAddress.RussianIndex))
                    russianAddress.Индекс = BuyerAddress.RussianIndex;

                if (!string.IsNullOrEmpty(BuyerAddress.RussianStreet))
                    russianAddress.Улица = BuyerAddress.RussianStreet;

                if (!string.IsNullOrEmpty(BuyerAddress.RussianHouse))
                    russianAddress.Дом = BuyerAddress.RussianHouse;

                if (!string.IsNullOrEmpty(BuyerAddress.RussianFlat))
                    russianAddress.Кварт = BuyerAddress.RussianFlat;

                xsdDocument.Документ.СвСчФакт.СвПокуп[0].Адрес.Item = russianAddress;
            }
            else if (BuyerAddress != null)
            {
                var foreignAddress = new АдрИнфТип();
                foreignAddress.КодСтр = BuyerAddress.CountryCode;
                foreignAddress.НаимСтран = BuyerAddress.CountryName;
                foreignAddress.АдрТекст = BuyerAddress.ForeignTextAddress;

                xsdDocument.Документ.СвСчФакт.СвПокуп[0].Адрес.Item = foreignAddress;
            }

            xsdDocument.Документ.СвСчФакт.ДенИзм = new ФайлДокументСвСчФактДенИзм();
            xsdDocument.Документ.СвСчФакт.ДенИзм.НаимОКВ = CurrencyName;
            xsdDocument.Документ.СвСчФакт.ДенИзм.КодОКВ = CurrencyCode;

            if (DeliveryDocuments.Count > 0)
                xsdDocument.Документ.СвСчФакт.ДокПодтвОтгрНом = DeliveryDocuments?
                    .Select(d => new РеквДокТип
                    {
                        РеквНаимДок = d.DocumentName,
                        РеквНомерДок = d.DocumentNumber,
                        РеквДатаДок = d.DocumentDate.ToString("dd.MM.yyyy")
                    })?.ToArray();

            xsdDocument.Документ.ТаблСчФакт = new ФайлДокументТаблСчФакт();

            if (Products != null && Products.Count > 0)
                xsdDocument.Документ.ТаблСчФакт.СведТов = Products?
                    .Select(p =>
                    {
                        decimal taxAmount = p.TaxAmount ?? 0;
                        decimal price = p.Price ?? 0;
                        decimal quantity = p.Quantity ?? 0;

                        var good = new ФайлДокументТаблСчФактСведТов
                        {
                            НомСтр = p.Number.ToString(),
                            НаимТов = p.Description,
                            НаимЕдИзм = p.UnitName,
                            ОКЕИ_Тов = p.UnitCode,
                            КолТов = quantity,
                            КолТовSpecified = true,
                            ЦенаТов = price,
                            ЦенаТовSpecified = true,
                            СтТовБезНДС = price * quantity,
                            СтТовБезНДСSpecified = true,
                            СумНал = new СумНДСТип()
                        };

                        if (taxAmount == 0)
                        {
                            good.СумНал.Item = new СумНДСТипБезНДС();
                            good.НалСт = ФайлДокументТаблСчФактСведТовНалСт.Item0;
                            good.СтТовУчНал = good.СтТовБезНДС;
                            good.СтТовУчНалSpecified = true;
                        }
                        else
                        {
                            good.СумНал.Item = taxAmount;
                            good.СтТовУчНал = good.СтТовБезНДС + taxAmount;
                            good.СтТовУчНалSpecified = true;

                            if (p.VatRate == 10)
                                good.НалСт = ФайлДокументТаблСчФактСведТовНалСт.Item10;
                            else if (p.VatRate == 20)
                                good.НалСт = ФайлДокументТаблСчФактСведТовНалСт.Item20;
                            else
                                good.НалСт = ФайлДокументТаблСчФактСведТовНалСт.НДСисчисляетсяналоговымагентом;

                        }

                        good.Акциз = new СумАкцизТип();
                        if (p.WithoutExcise)
                            good.Акциз.Item = СумАкцизТипБезАкциз.безакциза;
                        else
                            good.Акциз.Item = p.ExciseSumm;

                        if (!string.IsNullOrEmpty(p.CustomsDeclarationCode))
                        {
                            good.СвДТ = new ФайлДокументТаблСчФактСведТовСвДТ[1];
                            good.СвДТ[0] = new ФайлДокументТаблСчФактСведТовСвДТ();
                            good.СвДТ[0].КодПроисх = p.OriginCode;

                            if (p.OriginCode != codeOfRussia)
                                good.СвДТ[0].НомерДТ = p.CustomsDeclarationCode;
                        }

                        good.ДопСведТов = new ФайлДокументТаблСчФактСведТовДопСведТов();
                        good.ДопСведТов.КодТов = p.BarCode;
                        
                        if(!string.IsNullOrEmpty(p.OriginCountryName))
                            good.ДопСведТов.КрНаимСтрПр = new string[] { p.OriginCountryName };

                        if (p.MarkedCodes != null && p.MarkedCodes.Count > 0)
                        {
                            good.ДопСведТов.НомСредИдентТов = new ФайлДокументТаблСчФактСведТовДопСведТовНомСредИдентТов[1];
                            good.ДопСведТов.НомСредИдентТов[0] = new ФайлДокументТаблСчФактСведТовДопСведТовНомСредИдентТов();

                            good.ДопСведТов.НомСредИдентТов[0].Items = p.MarkedCodes.ToArray();
                            good.ДопСведТов.НомСредИдентТов[0].ItemsElementName = p.MarkedCodes.Select(m => ItemsChoiceType.КИЗ).ToArray();
                        }
                        else if (p.TransportPackingIdentificationCode != null && p.TransportPackingIdentificationCode.Count > 0)
                        {
                            good.ДопСведТов.НомСредИдентТов = p.TransportPackingIdentificationCode.Select(t => new ФайлДокументТаблСчФактСведТовДопСведТовНомСредИдентТов { ИдентТрансУпак = t }).ToArray();
                        }

                        return good;
                    })?.ToArray();

            xsdDocument.Документ.ТаблСчФакт.ВсегоОпл = new ФайлДокументТаблСчФактВсегоОпл();
            xsdDocument.Документ.ТаблСчФакт.ВсегоОпл.СтТовБезНДСВсего = Products?.Sum(p => (p.Quantity ?? 0) * (p.Price ?? 0)) ?? 0;
            xsdDocument.Документ.ТаблСчФакт.ВсегоОпл.СтТовБезНДСВсегоSpecified = true;

            var taxAmountTolal = Products?.Sum(p => (p.TaxAmount ?? 0)) ?? 0;

            xsdDocument.Документ.ТаблСчФакт.ВсегоОпл.СумНалВсего = new СумНДСТип();

            if (taxAmountTolal == 0)
            {
                xsdDocument.Документ.ТаблСчФакт.ВсегоОпл.СумНалВсего.Item = СумНДСТипБезНДС.безНДС;
                xsdDocument.Документ.ТаблСчФакт.ВсегоОпл.СтТовУчНалВсего = xsdDocument.Документ.ТаблСчФакт.ВсегоОпл.СтТовБезНДСВсего;
                xsdDocument.Документ.ТаблСчФакт.ВсегоОпл.СтТовУчНалВсегоSpecified = true;
            }
            else
            {
                xsdDocument.Документ.ТаблСчФакт.ВсегоОпл.СумНалВсего.Item = taxAmountTolal;
                xsdDocument.Документ.ТаблСчФакт.ВсегоОпл.СтТовУчНалВсего = xsdDocument.Документ.ТаблСчФакт.ВсегоОпл.СтТовБезНДСВсего + taxAmountTolal;
                xsdDocument.Документ.ТаблСчФакт.ВсегоОпл.СтТовУчНалВсегоSpecified = true;
            }

            if (!string.IsNullOrEmpty(ContentOperation))
            {
                xsdDocument.Документ.СвПродПер = new ФайлДокументСвПродПер();
                xsdDocument.Документ.СвПродПер.СвПер = new ФайлДокументСвПродПерСвПер();
                xsdDocument.Документ.СвПродПер.СвПер.СодОпер = ContentOperation;
                xsdDocument.Документ.СвПродПер.СвПер.ВидОпер = OperationType;

                if (ShippingDate != null)
                    xsdDocument.Документ.СвПродПер.СвПер.ДатаПер = ShippingDate.Value.ToString("dd.MM.yyyy");

                if (!string.IsNullOrEmpty(BasisDocumentName))
                {
                    var basisDocumentType = new РеквДокТип();

                    basisDocumentType.РеквНаимДок = BasisDocumentName;
                    basisDocumentType.РеквНомерДок = BasisDocumentNumber;
                    basisDocumentType.РеквДопСведДок = BasisDocumentOtherInfo;
                    basisDocumentType.РеквИдДок = BasisDocumentId;

                    if (BasisDocumentDate != null)
                        basisDocumentType.РеквДатаДок = BasisDocumentDate.Value.ToString("dd.MM.yyyy");

                    xsdDocument.Документ.СвПродПер.СвПер.Items = new object[]
                    {
                        basisDocumentType
                    };
                }
                else
                {
                    xsdDocument.Документ.СвПродПер.СвПер.Items = new object[] 
                    {
                        new ФайлДокументСвПродПерСвПерБезДокОснПер()
                    };
                }
            }

            xsdDocument.Документ.Подписант = new ФайлДокументПодписант[]
            {
                new ФайлДокументПодписант
                {
                    Должн = SignerPosition,
                    ДатаПодДок = SignDate?.ToString("dd.MM.yyyy")
                }
            };

            var signer = xsdDocument.Документ.Подписант[0];

            if (MethodOfConfirmingAuthority == MethodOfConfirmingAuthorityEnum.DigitalSignature)
                signer.СпосПодтПолном = ФайлДокументПодписантСпосПодтПолном.Item1;
            else if (MethodOfConfirmingAuthority == MethodOfConfirmingAuthorityEnum.EmchdInPackageOfElectronicDocuments)
                signer.СпосПодтПолном = ФайлДокументПодписантСпосПодтПолном.Item2;
            else if (MethodOfConfirmingAuthority == MethodOfConfirmingAuthorityEnum.EmchdDataInDocument)
            {
                signer.СпосПодтПолном = ФайлДокументПодписантСпосПодтПолном.Item3;

                if (ElectronicPowerOfAttorney == null)
                    throw new Exception("Не указана машиночитаемая доверенность.");

                signer.Items = new[] 
                {
                    new ФайлДокументПодписантСвДоверЭл
                    {
                        НомДовер = ElectronicPowerOfAttorney.RegistrationNumber,
                        ДатаВыдДовер = ElectronicPowerOfAttorney.RegistrationDate.ToString("dd.MM.yyyy"),
                        ИдСистХран = ElectronicPowerOfAttorney.SystemIdentificationInfo,
                        УРЛСист = ElectronicPowerOfAttorney.UrlSystem
                    }
                };
            }
            else if (MethodOfConfirmingAuthority == MethodOfConfirmingAuthorityEnum.EmchdDataInOtherSystem)
                signer.СпосПодтПолном = ФайлДокументПодписантСпосПодтПолном.Item4;
            else if (MethodOfConfirmingAuthority == MethodOfConfirmingAuthorityEnum.PaperPowerOfAttorney)
                signer.СпосПодтПолном = ФайлДокументПодписантСпосПодтПолном.Item5;
            else if (MethodOfConfirmingAuthority == MethodOfConfirmingAuthorityEnum.Other)
                signer.СпосПодтПолном = ФайлДокументПодписантСпосПодтПолном.Item6;

            if (SignType == SignTypeEnum.QualifiedElectronicDigitalSignature)
                signer.ТипПодпис = ФайлДокументПодписантТипПодпис.Item1;
            else if(SignType == SignTypeEnum.SimpleElectronicDigitalSignature)
                signer.ТипПодпис = ФайлДокументПодписантТипПодпис.Item2;
            else if (SignType == SignTypeEnum.NonQualifiedElectronicDigitalSignature)
                signer.ТипПодпис = ФайлДокументПодписантТипПодпис.Item3;

            signer.ФИО = new ФИОТип
            {
                Фамилия = SignerSurname,
                Имя = SignerName,
                Отчество = SignerPatronymic
            };

            string xml = Xml.SerializeEntity<Файл>(xsdDocument, Encoding.GetEncoding(1251));
            return $"<?xml version=\"1.0\" encoding=\"windows-1251\"?>{xml}";
        }
        #endregion
    }
}
