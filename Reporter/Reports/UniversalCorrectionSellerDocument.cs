using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UtilitesLibrary.Service;
using Reporter.XsdClasses.OnNkorschfdoppr;

namespace Reporter.Reports
{
    public class UniversalCorrectionSellerDocument : IReport
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
        public string ProviderInn { get; set; }

        /// <summary>
        /// Идентификатор оператора электронного документооборота отправителя файла обмена корректировочного счета-фактуры с дополнительной информацией (информации продавца)
        /// </summary>
        public string EdoId { get; set; }

        /// <summary>
        /// Идентификатор участника документооборота - отправителя файла обмена корректировочного счета-фактуры с дополнительной информацией (информации продавца)
        /// </summary>
        public string SenderEdoId { get; set; }

        /// <summary>
        /// Идентификатор участника документооборота, получателя файла обмена корректировочного счета-фактуры с дополнительной информацией (информации продавца)
        /// </summary>
        public string ReceiverEdoId { get; set; }
        #endregion

        #region Документ
        /// <summary>
        /// Дата формирования файла обмена корректировочного счета-фактуры с дополнительной информацией (информации продавца)
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

        #region Сведения о корректировочном счете-фактуре с дополнительной информацией
        /// <summary>
        /// Порядковый номер корректировочного счета-фактуры (строка 1 корректировочного счета-фактуры) и (или) документа о согласии покупателя на изменение стоимости отгрузки
        /// </summary>
        public string DocNumber { get; set; }

        /// <summary>
        /// Дата составления  (выписки) корректировочного счета-фактуры (строка 1 корректировочного счета-фактуры) и (или) документа о согласии покупателя на изменение стоимости отгрузки
        /// </summary>
        public DateTime DocDate { get; set; }

        /// <summary>
        /// Счет-фактура, счет-фактура с дополнительной информацией, документ о передаче товаров (работ, услуг, имущественных прав), в результате которой изменяется финансовое состояние передающей и принимающей стороны, к которому составлен корректировочный документ
        /// </summary>
        public List<Entities.InvoiceCorrectionInfo> CorrectionInvoices { get; set; }

        #region Сведения о продавце (строки 2, 2а, 2б корректировочного счета-фактуры)

        /// <summary>
        /// Наименование продавца
        /// </summary>
        public string SellerName { get; set; }

        /// <summary>
        /// ИНН продавца
        /// </summary>
        public string SellerInn { get; set; }

        /// <summary>
        /// КПП продавца
        /// </summary>
        public string SellerKpp { get; set; }
        #endregion

        #region Сведения о покупателе (строки 3, 3а, 3б корректировочного счета-фактуры)

        /// <summary>
        /// Наименование покупателя
        /// </summary>
        public string BuyerName { get; set; }

        /// <summary>
        /// ИНН покупателя
        /// </summary>
        public string BuyerInn { get; set; }

        /// <summary>
        /// КПП покупателя
        /// </summary>
        public string BuyerKpp { get; set; }
        #endregion
        #endregion
        #region Сведения таблицы корректировочного счета-фактуры с дополнительной информацией

        /// <summary>
        /// Сведения о товаре (работе, услуге), имущественном праве
        /// </summary>
        public List<Entities.CorrectionProduct> Products { get; set; }
        #endregion
        #region Реквизиты строки «Всего увеличение» (сумма строк (В) по графам 5, 8 и 9 корректировочного счета-фактуры)

        /// <summary>
        /// Всего увеличение, сумма налога (строка «Всего увеличение»/ графа 8 корректировочного счета-фактуры)
        /// </summary>
        public decimal TaxAmountIncrease { get; set; }

        /// <summary>
        /// Всего увеличение, стоимость товаров (работ, услуг), имущественных прав с налогом - всего (строка «Всего увеличение»/графа 9 корректировочного счета-фактуры)
        /// </summary>
        public decimal SubtotalIncrease { get; set; }
        #endregion
        #region Реквизиты строки «Всего уменьшение» (сумма строк (Г) по графам 5, 8 и 9 корректировочного счета-фактуры)

        /// <summary>
        /// Всего уменьшение, сумма налога (строка «Всего уменьшение»/ графа 8 корректировочного счета-фактуры)
        /// </summary>
        public decimal TaxAmountDecrease { get; set; }

        /// <summary>
        /// Всего уменьшение, стоимость товаров (работ, услуг), имущественных прав, с налогом - всего (строка «Всего уменьшение»/графа 9 корректировочного счета-фактуры)
        /// </summary>
        public decimal SubtotalDecrease { get; set; }
        #endregion
        #region Содержание события (факта хозяйственной жизни) 3 – сведения о факте согласования (уведомления)

        /// <summary>
        /// Содержание операции
        /// </summary>
        public string EventDescription { get; set; }

        /// <summary>
        /// Дата направления на согласование (дата уведомления)
        /// </summary>
        public DateTime ApprovalDate { get; set; }

        /// <summary>
        /// Реквизиты передаточных (отгрузочных) документов, к которым относится корректировка
        /// </summary>
        public List<Entities.DocumentDetail> TransferDocuments { get; set; }

        /// <summary>
        /// Реквизиты документов, являющихся основанием корректировки
        /// </summary>
        public List<Entities.DocumentDetail> BasisForCorrection { get; set; }
        #endregion
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

            var infoAboutParticipants = xsdDocument?.СвУчДокОбор;

            if(infoAboutParticipants != null)
            {
                EdoProviderOrgName = infoAboutParticipants?.СвОЭДОтпр?.НаимОрг;
                ProviderInn = infoAboutParticipants?.СвОЭДОтпр?.ИННЮЛ;
                EdoId = infoAboutParticipants?.СвОЭДОтпр?.ИдЭДО;
                SenderEdoId = infoAboutParticipants.ИдОтпр;
                ReceiverEdoId = infoAboutParticipants.ИдПол;
            }

            var document = xsdDocument.Документ;
            if(document != null)
            {
                if (!(string.IsNullOrEmpty(document.ДатаИнфПр) || string.IsNullOrEmpty(document.ВремИнфПр)))
                    CreateDate = DateTime.ParseExact($"{document.ДатаИнфПр} {document.ВремИнфПр}", "dd.MM.yyyy HH.mm.ss", System.Globalization.CultureInfo.InvariantCulture);

                if (document.Функция == ФайлДокументФункция.ДИС)
                    Function = "ДИС";
                else if (document.Функция == ФайлДокументФункция.КСЧФ)
                    Function = "КСЧФ";
                else if (document.Функция == ФайлДокументФункция.КСЧФДИС)
                    Function = "КСЧФДИС";
                else if (document.Функция == ФайлДокументФункция.СвИСЗК)
                    Function = "СвИСЗК";
                else if (document.Функция == ФайлДокументФункция.СвИСРК)
                    Function = "СвИСРК";

                DocName = document.НаимДокОпр;

                var infoAboutCorrectionInvoice = document.СвКСчФ;

                if(infoAboutCorrectionInvoice != null)
                {
                    DocNumber = infoAboutCorrectionInvoice.НомерКСчФ;

                    if (!string.IsNullOrEmpty(infoAboutCorrectionInvoice.ДатаКСчФ))
                        DocDate = DateTime.ParseExact(infoAboutCorrectionInvoice.ДатаКСчФ, "dd.MM.yyyy", System.Globalization.CultureInfo.InvariantCulture);

                    if (infoAboutCorrectionInvoice.СчФ != null)
                    {
                        CorrectionInvoices = new List<Entities.InvoiceCorrectionInfo>();

                        foreach(var info in infoAboutCorrectionInvoice.СчФ)
                        {
                            CorrectionInvoices.Add(new Entities.InvoiceCorrectionInfo
                            {
                                DocNumber = info.НомерСчФ,
                                DocDate = DateTime.ParseExact(info.ДатаСчФ, "dd.MM.yyyy", System.Globalization.CultureInfo.InvariantCulture)
                            });
                        }
                    }
                }

                if(document?.ТаблКСчФ?.СведТов != null)
                {
                    Products = new List<Entities.CorrectionProduct>();

                    foreach (var pr in document?.ТаблКСчФ?.СведТов)
                    {
                        var product = new Entities.CorrectionProduct
                        {
                            BarCode = pr?.ДопСведТов?.КодТов,
                            Number = Convert.ToInt32(pr?.НомСтр),
                            Description = pr.НаимТов,

                            PriceBefore = pr.ЦенаТовДо,
                            QuantityBefore = pr.КолТовДо,
                            TaxAmountBefore = pr.СумНалДо?.Item as decimal?,
                            SubtotalBefore = pr.СтТовУчНал?.СтоимДоИзм,

                            PriceAfter = pr.ЦенаТовПосле,
                            QuantityAfter = pr.КолТовПосле,
                            TaxAmountAfter = pr.СумНалПосле?.Item as decimal?,
                            SubtotalAfter = pr.СтТовУчНал?.СтоимПослеИзм,
                        };

                        product.MarkedCodesBefore = new List<string>();
                        product.TransportPackingIdentificationCodeBefore = new List<string>();

                        if (pr.НомСредИдентТовДо != null)
                        {
                            foreach(var c in pr.НомСредИдентТовДо)
                            {
                                if (!string.IsNullOrEmpty(c.ИдентТрансУпак))
                                    product.TransportPackingIdentificationCodeBefore.Add(c.ИдентТрансУпак);

                                if (c.Items != null)
                                    product.MarkedCodesBefore.AddRange(c.Items);
                            }
                        }

                        product.MarkedCodesAfter = new List<string>();
                        product.TransportPackingIdentificationCodeAfter = new List<string>();

                        if(pr.НомСредИдентТовПосле != null)
                        {
                            foreach(var c in pr.НомСредИдентТовПосле)
                            {
                                if (!string.IsNullOrEmpty(c.ИдентТрансУпак))
                                    product.TransportPackingIdentificationCodeAfter.Add(c.ИдентТрансУпак);

                                if (c.Items != null)
                                    product.MarkedCodesAfter.AddRange(c.Items);
                            }
                        }

                        Products.Add(product);
                    }
                }

                var sellerInfo = infoAboutCorrectionInvoice?.СвПрод?.ИдСв;

                if(sellerInfo?.Item != null)
                {
                    if(sellerInfo.Item.GetType() == typeof(СвИПТип))
                    {
                        SellerName = $"ИП {((СвИПТип)sellerInfo.Item)?.ФИО?.Фамилия} {((СвИПТип)sellerInfo.Item)?.ФИО?.Имя} {((СвИПТип)sellerInfo.Item)?.ФИО?.Отчество}";
                        SellerInn = ((СвИПТип)sellerInfo.Item).ИННФЛ;
                    }
                    else if(sellerInfo.Item.GetType() == typeof(СвПродПокТипИдСвСвИнНеУч))
                    {
                        SellerName = ((СвПродПокТипИдСвСвИнНеУч)sellerInfo.Item).НаимОрг;
                    }
                    else if (sellerInfo.Item.GetType() == typeof(СвПродПокТипИдСвСвЮЛУч))
                    {
                        SellerName = ((СвПродПокТипИдСвСвЮЛУч)sellerInfo.Item).НаимОрг;
                        SellerInn = ((СвПродПокТипИдСвСвЮЛУч)sellerInfo.Item).ИННЮЛ;
                        SellerKpp = ((СвПродПокТипИдСвСвЮЛУч)sellerInfo.Item).КПП;
                    }
                }

                var buyerInfo = infoAboutCorrectionInvoice?.СвПокуп?.ИдСв;
                if (buyerInfo?.Item != null)
                {
                    if(buyerInfo.Item.GetType() == typeof(СвИПТип))
                    {
                        BuyerName = $"ИП {((СвИПТип)buyerInfo.Item)?.ФИО?.Фамилия} {((СвИПТип)buyerInfo.Item)?.ФИО?.Имя} {((СвИПТип)buyerInfo.Item)?.ФИО?.Отчество}";
                        BuyerInn = ((СвИПТип)buyerInfo.Item).ИННФЛ;
                    }
                    else if (buyerInfo.Item.GetType() == typeof(СвПродПокТипИдСвСвИнНеУч))
                    {
                        BuyerName = ((СвПродПокТипИдСвСвИнНеУч)buyerInfo.Item).НаимОрг;
                    }
                    else if (buyerInfo.Item.GetType() == typeof(СвПродПокТипИдСвСвЮЛУч))
                    {
                        BuyerName = ((СвПродПокТипИдСвСвЮЛУч)buyerInfo.Item).НаимОрг;
                        BuyerInn = ((СвПродПокТипИдСвСвЮЛУч)buyerInfo.Item).ИННЮЛ;
                        BuyerKpp = ((СвПродПокТипИдСвСвЮЛУч)buyerInfo.Item).КПП;
                    }
                }

                TaxAmountIncrease = ((decimal?)document?.ТаблКСчФ?.ВсегоУвел?.СумНал?.Item) ?? 0;
                SubtotalIncrease = document?.ТаблКСчФ?.ВсегоУвел?.СтТовУчНалВсего ?? 0;

                TaxAmountDecrease = ((decimal?)document?.ТаблКСчФ?.ВсегоУм?.СумНал?.Item) ?? 0;
                SubtotalDecrease = document?.ТаблКСчФ?.ВсегоУм?.СтТовУчНалВсего ?? 0;

                var eventContentFactOfEconomicLife = document?.СодФХЖ3;

                if(eventContentFactOfEconomicLife != null)
                {
                    EventDescription = eventContentFactOfEconomicLife.СодОпер;

                    if(!string.IsNullOrEmpty(eventContentFactOfEconomicLife.ДатаНапр))
                        ApprovalDate = DateTime.ParseExact(eventContentFactOfEconomicLife.ДатаНапр, "dd.MM.yyyy", System.Globalization.CultureInfo.InvariantCulture);

                    TransferDocuments = new List<Entities.DocumentDetail>();
                    foreach (var doc in eventContentFactOfEconomicLife.ПередатДокум)
                    {
                        var docDetail = new Entities.DocumentDetail
                        {
                            DocName = doc.НаимОсн,
                            DocNumber = doc.НомОсн,
                            OtherInfo = doc.ДопСвОсн,
                            FileId = doc.ИдФайлОсн
                        };

                        if (!string.IsNullOrEmpty(doc.ДатаОсн))
                            docDetail.DocDate = DateTime.ParseExact(doc.ДатаОсн, "dd.MM.yyyy", System.Globalization.CultureInfo.InvariantCulture);

                        TransferDocuments.Add(docDetail);
                    }

                    BasisForCorrection = new List<Entities.DocumentDetail>();
                    foreach (var doc in eventContentFactOfEconomicLife.ДокумОснКор)
                    {
                        var docDetail = new Entities.DocumentDetail
                        {
                            DocName = doc.НаимОсн,
                            DocNumber = doc.НомОсн,
                            OtherInfo = doc.ДопСвОсн,
                            FileId = doc.ИдФайлОсн
                        };

                        if (!string.IsNullOrEmpty(doc.ДатаОсн))
                            docDetail.DocDate = DateTime.ParseExact(doc.ДатаОсн, "dd.MM.yyyy", System.Globalization.CultureInfo.InvariantCulture);

                        BasisForCorrection.Add(docDetail);
                    }
                }
            }
        }
        #endregion

        #region GetXmlContent
        public string GetXmlContent()
        {
            return null;
        }
        #endregion
    }
}
