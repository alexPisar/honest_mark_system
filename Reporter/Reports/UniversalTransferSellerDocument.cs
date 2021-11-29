using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UtilitesLibrary.Service;
using Reporter.Entities;
using Reporter.XsdClasses.OnNschfdoppr;

namespace Reporter.Reports
{
    public class UniversalTransferSellerDocument : IReport
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
        /// Дата составления (выписки) счета-фактуры (строка 1 счета-фактуры), документа об отгрузке товаров (выполнении работ), передаче имущественных прав (документа об оказании услуг)
        /// </summary>
        public DateTime DocDate { get; set; }
        #endregion

        #region Сведения таблицы счета-фактуры (содержание факта хозяйственной жизни 2 - наименование и другая информация об отгруженных товарах (выполненных работах, оказанных услугах), о переданных имущественных правах
        #region Сведения о товарах
        public List<Product> Products { get; set; }
        #endregion

        #region Всего к оплате

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

            if (infoAboutParticipants != null)
            {
                EdoProviderOrgName = infoAboutParticipants?.СвОЭДОтпр?.НаимОрг;
                ProviderInn = infoAboutParticipants?.СвОЭДОтпр?.ИННЮЛ;
                EdoId = infoAboutParticipants?.СвОЭДОтпр?.ИдЭДО;
                SenderEdoId = infoAboutParticipants.ИдОтпр;
                ReceiverEdoId = infoAboutParticipants.ИдПол;
            }

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
                DocNumber = document.СвСчФакт?.НомерСчФ;

                if (!string.IsNullOrEmpty(document.СвСчФакт?.ДатаСчФ))
                    DocDate = DateTime.ParseExact(document.СвСчФакт.ДатаСчФ, "dd.MM.yyyy", System.Globalization.CultureInfo.InvariantCulture);

                var goods = document.ТаблСчФакт?.СведТов ?? new ФайлДокументТаблСчФактСведТов[] { };
                Products = new List<Product>();

                foreach (var good in goods)
                {
                    var product = new Product()
                    {
                        Description = good.НаимТов,
                        Quantity = good.КолТов
                    };

                    product.MarkedCodes = new List<string>();
                    product.TransportPackingIdentificationCode = new List<string>();

                    if (good?.ДопСведТов?.НомСредИдентТов != null)
                    {
                        foreach (var code in good.ДопСведТов.НомСредИдентТов)
                        {
                            if (!string.IsNullOrEmpty(code.ИдентТрансУпак))
                                product.TransportPackingIdentificationCode.Add(code.ИдентТрансУпак);

                            if(code.Items != null)
                                product.MarkedCodes.AddRange(code.Items);
                        }
                    }

                    Products.Add(product);
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
