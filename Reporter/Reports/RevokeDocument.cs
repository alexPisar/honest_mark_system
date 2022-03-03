using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UtilitesLibrary.Service;
using Reporter.XsdClasses.DpPrannul;
using Reporter.Entities;

namespace Reporter.Reports
{
    public class RevokeDocument : IReport
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
        #region Сведение об участнике электронного документооборота
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
        public string JuridicalCreatorInn { get; set; }

        /// <summary>
        /// КПП организации
        /// </summary>
        public string JuridicalCreatorKpp { get; set; }

        /// <summary>
        /// Наименование организации
        /// </summary>
        public string OrgCreatorName { get; set; }
        #endregion
        #region Общие сведения предложения об аннулировании электронного документа
        #region Сведения по полученному файлу

        /// <summary>
        /// ЭЦП под полученным файлом
        /// </summary>
        public string Signature { get; set; }

        /// <summary>
        /// Имя поступившего файла
        /// </summary>
        public string ReceivedFileName { get; set; }
        #endregion

        /// <summary>
        /// Текст предложения об аннулировании
        /// </summary>
        public string Text { get; set; }
        #endregion
        #region Сведения об участнике информационного обмена, кому направляется предложение об аннулировании
        /// <summary>
        /// Идентификатор участника документооборота
        /// </summary>
        public string ReceiverEdoId { get; set; }

        /// <summary>
        /// Сведения об индивидуальном предпринимателе
        /// </summary>
        public IndividualEntity IndividualReceiver { get; set; }

        /// <summary>
        /// ИНН организации
        /// </summary>
        public string JuridicalReceiverInn { get; set; }

        /// <summary>
        /// КПП организации
        /// </summary>
        public string JuridicalReceiverKpp { get; set; }

        /// <summary>
        /// Наименование организации
        /// </summary>
        public string OrgReceiverName { get; set; }
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

            document.Документ = new ФайлДокумент();
            document.Документ.УчастЭДО = new УчастЭДОТип();
            document.Документ.УчастЭДО.ИдУчастЭДО = CreatorEdoId;

            if(IndividualCreator != null)
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
                    ИННЮЛ = JuridicalCreatorInn,
                    КПП = JuridicalCreatorKpp,
                    НаимОрг = OrgCreatorName
                };
            }

            document.Документ.СвПредАн = new ФайлДокументСвПредАн();
            document.Документ.СвПредАн.ТекстПредАн = Text;

            document.Документ.СвПредАн.СведАнФайл = new ФайлДокументСвПредАнСведАнФайл();
            document.Документ.СвПредАн.СведАнФайл.ИмяАнФайла = ReceivedFileName;
            document.Документ.СвПредАн.СведАнФайл.ЭЦПАнФайл = new string[] { Signature };

            document.Документ.НапрПредАн = new УчастЭДОТип();
            document.Документ.НапрПредАн.ИдУчастЭДО = ReceiverEdoId;

            if(IndividualReceiver != null)
            {
                document.Документ.НапрПредАн.Item = new ФЛТип
                {
                    ИННФЛ = IndividualReceiver.Inn,
                    ФИО = new ФИОТип
                    {
                        Фамилия = IndividualReceiver.Surname,
                        Имя = IndividualReceiver.Name,
                        Отчество = IndividualReceiver.Patronymic
                    }
                };
            }
            else
            {
                document.Документ.НапрПредАн.Item = new ЮЛТип
                {
                    ИННЮЛ = JuridicalReceiverInn,
                    КПП = JuridicalReceiverKpp,
                    НаимОрг = OrgReceiverName
                };
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
