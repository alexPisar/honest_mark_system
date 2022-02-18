using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Reporter.Entities;
using UtilitesLibrary.Service;
using Reporter.XsdClasses.DpUvutoch;

namespace Reporter.Reports
{
    public class ClarificationCorrectionRequestDocument : IReport
    {
        #region Properties
        #region Общая информация

        /// <summary>
        /// Идентификатор файла
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// Версия программы, с помощью которой сформирован файл
        /// </summary>
        public string EdoProgramVersion { get; set; }

        #endregion

        #region Участник электронного документооборота в рамках выставления и получения счетов-фактур, сформировавший уведомление об уточнении

        /// <summary>
        /// Идентификатор участника документооборота
        /// </summary>
        public string CreatorEdoId { get; set; }

        /// <summary>
        /// Сведения об индивидуальном предпринимателе
        /// </summary>
        public IndividualEntity IndividualCreator { get; set; }

        /// <summary>
        /// ИНН организации
        /// </summary>
        public string JuridicalInn { get; set; }

        /// <summary>
        /// КПП организации
        /// </summary>
        public string JuridicalKpp { get; set; }

        /// <summary>
        /// Наименование организации
        /// </summary>
        public string OrgCreatorName { get; set; }

        #endregion

        #region Общие сведения уведомления об уточнении электронного документа

        /// <summary>
        /// Дата получения
        /// </summary>
        public DateTime? ReceiveDate { get; set; }

        /// <summary>
        /// Текст уведомления об уточнении
        /// </summary>
        public string Text { get; set; }

        #region Сведения по полученному файлу

        /// <summary>
        /// Имя поступившего  файла
        /// </summary>
        public string ReceivedFileName { get; set; }

        /// <summary>
        /// ЭЦП под полученным файлом
        /// </summary>
        public string ReceivedFileSignature { get; set; }

        #endregion
        #endregion

        #region Отправитель документа

        /// <summary>
        /// Идентификатор участника документооборота
        /// </summary>
        public string SenderEdoId { get; set; }

        /// <summary>
        /// Сведения об индивидуальном предпринимателе
        /// </summary>
        public IndividualEntity IndividualSender { get; set; }

        /// <summary>
        /// ИНН организации
        /// </summary>
        public string SenderJuridicalInn { get; set; }

        /// <summary>
        /// КПП организации
        /// </summary>
        public string SenderJuridicalKpp { get; set; }

        /// <summary>
        /// Наименование организации-отправителя
        /// </summary>
        public string OrgSenderName { get; set; }

        #endregion

        #region Подписант

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
        public void Parse(string content)
        {

        }

        public void Parse(byte[] content)
        {
            var xmlString = Encoding.GetEncoding(1251).GetString(content);
            Parse(xmlString);
        }
        #endregion

        #region GetXmlContentMethods
        public string GetXmlContent()
        {
            var document = new Файл();

            document.ИдФайл = FileName;
            document.ВерсПрог = EdoProgramVersion;
            document.ВерсФорм = ФайлВерсФорм.Item102;

            document.Документ = new ФайлДокумент();
            document.Документ.УчастЭДО = new УчастЭДОТип();
            document.Документ.УчастЭДО.ИдУчастЭДО = CreatorEdoId;

            if (IndividualCreator != null)
            {
                document.Документ.УчастЭДО.Item = new ФЛТип()
                {
                    ИННФЛ = IndividualCreator.Inn,
                    ФИО = new ФИОТип
                    {
                        Фамилия = IndividualCreator.Surname,
                        Имя = IndividualCreator.Name,
                        Отчество = IndividualCreator.Patronymic
                    }
                };
            }
            else
            {
                document.Документ.УчастЭДО.Item = new ЮЛТип()
                {
                    НаимОрг = OrgCreatorName,
                    ИННЮЛ = JuridicalInn
                };

                if (!string.IsNullOrEmpty(JuridicalKpp))
                    ((ЮЛТип)document.Документ.УчастЭДО.Item).КПП = JuridicalKpp;
            }

            document.Документ.СвУведУточ = new ФайлДокументСвУведУточ();
            
            if(ReceiveDate != null)
            {
                document.Документ.СвУведУточ.ДатаПол = ReceiveDate.Value.ToString("dd.MM.yyyy");
                document.Документ.СвУведУточ.ВремяПол = ReceiveDate.Value.ToString("HH.mm.ss");
            }

            document.Документ.СвУведУточ.ТекстУведУточ = Text;

            document.Документ.СвУведУточ.СведПолФайл = new ФайлДокументСвУведУточСведПолФайл
            {
                ИмяПостФайла = ReceivedFileName,
                ЭЦППолФайл = ReceivedFileSignature
            };

            document.Документ.ОтпрДок = new УчастЭДОТип();
            document.Документ.ОтпрДок.ИдУчастЭДО = SenderEdoId;
            
            if(IndividualSender != null)
            {
                document.Документ.ОтпрДок.Item = new ФЛТип
                {
                    ИННФЛ = IndividualSender.Inn,
                    ФИО = new ФИОТип
                    {
                        Фамилия = IndividualSender.Surname,
                        Имя = IndividualSender.Name,
                        Отчество = IndividualSender.Patronymic
                    }
                };
            }
            else
            {
                document.Документ.ОтпрДок.Item = new ЮЛТип
                {
                    ИННЮЛ = SenderJuridicalInn,
                    НаимОрг = OrgSenderName
                };

                if (!string.IsNullOrEmpty(SenderJuridicalKpp))
                    ((ЮЛТип)document.Документ.ОтпрДок.Item).КПП = SenderJuridicalKpp;
            }

            document.Документ.Подписант = new ПодписантТип
            {
                Должность = SignerPosition,
                ФИО = new ФИОТип
                {
                    Фамилия = SignerSurname,
                    Имя = SignerName,
                    Отчество = SignerPatronymic
                }
            };

            string xml = Xml.SerializeEntity<Файл>(document, Encoding.GetEncoding(1251));
            return $"<?xml version=\"1.0\" encoding=\"windows-1251\"?>{xml}";
        }
        #endregion
    }
}
