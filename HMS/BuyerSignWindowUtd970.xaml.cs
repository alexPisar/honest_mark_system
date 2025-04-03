using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Reporter.Reports;
using UtilitesLibrary.Service;

namespace HonestMarkSystem
{
    /// <summary>
    /// Логика взаимодействия для BuyerSignWindowUtd970.xaml
    /// </summary>
    public partial class BuyerSignWindowUtd970 : BaseControls.BaseBuyerSignWindow
    {
        private string _prefixBuyerFileName = "ON_NSCHFDOPPOK";
        private string _prefixSellerFileName = "ON_NSCHFDOPPR";
        private CryptoUtil _cryptoUtil;
        private UtilityLog _log = UtilityLog.GetInstance();
        private bool _isMarked;

        private UniversalTransferBuyerDocumentUtd970 _report => Report as UniversalTransferBuyerDocumentUtd970;

        public BuyerSignWindowUtd970(CryptoUtil cryptoUtil, string filePath)
        {
            InitializeComponent();
            _cryptoUtil = cryptoUtil;
            _filePath = filePath;
            reportControl.OnCancelButtonClick += CancelButtonClick;
            reportControl.OnChangeButtonClick += SendButtonClick;
            reportControl.OnSaveButtonClick += SaveButtonClick;
        }

        public override void OnAllPropertyChanged()
        {
            reportControl.OnAllPropertyCnanged();
        }

        public override string FileName => _report?.FileName;

        public override bool IsMarked => _isMarked;

        public override Reporter.IReport Report
        {
            get {
                return (UniversalTransferBuyerDocumentUtd970)reportControl.Report;
            }
        }

        public void SetReport(UniversalTransferBuyerDocumentUtd970 report)
        {
            reportControl.DataContext = report;
        }

        public void ReportDataChanged()
        {
            reportControl.SetDefaults();
        }

        private void CancelButtonClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void SaveButtonClick(object sender, RoutedEventArgs e)
        {
            var changePathDialog = new Microsoft.Win32.SaveFileDialog();
            changePathDialog.Title = "Сохранение файла";
            changePathDialog.Filter = "XML Files|*.xml";

            try
            {
                _report.Signature = GetSignatureStringForReport();
                _report.DateReceive = DateTime.Now;
                changePathDialog.FileName = _report.FileName;

                if (_report.AcceptResult == Reporter.Enums.AcceptResultEnum.GoodsAcceptedWithoutDiscrepancy)
                {
                    _report.ContentOperationText = "Товары (работы, услуги, права) приняты без расхождений (претензий)";
                }
                else if (_report.AcceptResult == Reporter.Enums.AcceptResultEnum.GoodsAcceptedWithDiscrepancy)
                {
                    _report.ContentOperationText = "Товары (работы, услуги, права) приняты с расхождениями (претензией)";
                }
                else if (_report.AcceptResult == Reporter.Enums.AcceptResultEnum.GoodsNotAccepted)
                {
                    _report.ContentOperationText = "Товары (работы, услуги, права) не приняты";
                }

                if (changePathDialog.ShowDialog() ?? false)
                {
                    var xml = Report.GetXmlContent();
                    var content = Encoding.GetEncoding(1251).GetBytes(xml);
                    System.IO.File.WriteAllBytes(changePathDialog.FileName, content);

                    var loadWindow = new LoadWindow();
                    loadWindow.AfterSuccessfullLoading("Файл успешно сохранён.");
                    loadWindow.ShowDialog();

                    _log.Log("Файл Xml покупателя успешно сохранён.");
                }
            }
            catch(Exception ex)
            {
                var errorWindow = new ErrorsWindow(
                        "Произошла ошибка сохранения файла УПД покупателя.",
                        new List<string>(
                            new string[]
                            {
                                    ex.Message,
                                    ex.StackTrace
                            }
                            ));

                errorWindow.ShowDialog();
                _log.Log("Exception: " + _log.GetRecursiveInnerException(ex));
            }
        }

        private void SendButtonClick(object sender, RoutedEventArgs e)
        {
            bool validationResult = reportControl.ValidateReport();

            if (validationResult)
            {
                _report.Signature = GetSignatureStringForReport();
                _report.DateReceive = DateTime.Now;

                if (_report.AcceptResult == Reporter.Enums.AcceptResultEnum.GoodsAcceptedWithoutDiscrepancy)
                {
                    _report.ContentOperationText = "Товары (работы, услуги, права) приняты без расхождений (претензий)";
                }
                else if (_report.AcceptResult == Reporter.Enums.AcceptResultEnum.GoodsAcceptedWithDiscrepancy)
                {
                    _report.ContentOperationText = "Товары (работы, услуги, права) приняты с расхождениями (претензией)";
                }
                else if (_report.AcceptResult == Reporter.Enums.AcceptResultEnum.GoodsNotAccepted)
                {
                    _report.ContentOperationText = "Товары (работы, услуги, права) не приняты";
                }

                if (reportControl.ValidateXml())
                {
                    DialogResult = true;
                    Close();
                }
                else
                {
                    var errorsWindow = new ErrorsWindow("Ошибка валидации файла по xsd схеме.", reportControl.ErrorsValidationXml);
                    errorsWindow.ShowDialog();
                }
            }
            else
            {
                var errorsWindow = new ErrorsWindow("Ошибка валидации", reportControl.ErrorsValidationReport);
                errorsWindow.ShowDialog();
            }
        }

        public override void SetDefaultParameters(Models.ConsignorOrganization organization, string subject, DataContextManagementUnit.DataAccess.Contexts.Abt.DocEdoPurchasing dataBaseObject, string edoProgramVersion)
        {
            _report.CreateBuyerFileDate = DateTime.Now;

            _report.SignerInfo = new Reporter.Entities.SignerInfo
            {
                SignType = Reporter.Enums.SignTypeEnum.QualifiedElectronicDigitalSignature,
                SignDate = DateTime.Now
            };

            var orgInn = organization.OrgInn;
            var orgName = organization.OrgName;
            if (!string.IsNullOrEmpty(organization.EmchdId))
            {
                _report.SignerInfo.Position = organization.EmchdPersonPosition;
                _report.SignerInfo.Surname = organization.EmchdPersonSurname;
                _report.SignerInfo.Name = organization.EmchdPersonName;
                _report.SignerInfo.Patronymic = organization.EmchdPersonPatronymicSurname;
                _report.SignerInfo.MethodOfConfirmingAuthorityEnum = Reporter.Enums.MethodOfConfirmingAuthorityEnum.EmchdDataInDocument;

                _report.SignerInfo.PowerOfAttorney = new Reporter.Entities.ElectronicPowerOfAttorney
                {
                    RegistrationNumber = organization.EmchdId,
                    SystemIdentificationInfo = $"https://m4d.nalog.gov.ru/emchd/check-status?guid={organization.EmchdId}"
                };

                if (organization.EmchdBeginDate != null)
                    (_report.SignerInfo.PowerOfAttorney as Reporter.Entities.ElectronicPowerOfAttorney).RegistrationDate = organization.EmchdBeginDate.Value;

                _report.FinSubjectCreator = $"{organization.EmchdPersonSurname} {organization.EmchdPersonName} {organization.EmchdPersonPatronymicSurname}";
            }
            else
            {
                var firstMiddleName = _cryptoUtil.ParseCertAttribute(subject, "G");

                _report.SignerInfo.Surname = _cryptoUtil.ParseCertAttribute(subject, "SN");
                _report.SignerInfo.Name = firstMiddleName.IndexOf(" ") > 0 ? firstMiddleName.Substring(0, firstMiddleName.IndexOf(" ")) : string.Empty;
                _report.SignerInfo.Patronymic = firstMiddleName.IndexOf(" ") >= 0 && firstMiddleName.Length > firstMiddleName.IndexOf(" ") + 1 ? firstMiddleName.Substring(firstMiddleName.IndexOf(" ") + 1) : string.Empty;
                _report.SignerInfo.Position = _cryptoUtil.ParseCertAttribute(subject, "T");
                _report.SignerInfo.MethodOfConfirmingAuthorityEnum = Reporter.Enums.MethodOfConfirmingAuthorityEnum.DigitalSignature;
                _report.FinSubjectCreator = $"{orgName}, ИНН: {orgInn}";
            }

            //if (string.IsNullOrEmpty(orgInn))
            //    orgInn = ((Reporter.Entities.IndividualEntity)Report.SignerEntity).Inn;

            _report.AcceptResult = Reporter.Enums.AcceptResultEnum.GoodsAcceptedWithoutDiscrepancy;

            _report.SellerFileId = dataBaseObject.FileName;

            if (!string.IsNullOrEmpty(_report.SellerFileId) && _report.SellerFileId.Length > 13)
            {
                var endOfSellerFile = _report.SellerFileId.Substring(_report.SellerFileId.Length - 13);

                if (dataBaseObject != null)
                    _report.FileName = $"{_prefixBuyerFileName}_{dataBaseObject.SenderEdoId}_{dataBaseObject.ReceiverEdoId}_{DateTime.Now.ToString("yyyyMMdd")}_{Guid.NewGuid().ToString()}{endOfSellerFile}";

                _isMarked = endOfSellerFile[3] == '1';
            }

            var reporterDll = new Reporter.ReporterDll();
            var sellerReport = reporterDll.ParseDocument<UniversalTransferSellerDocumentUtd970>(DocSellerContent);

            _report.CreateSellerFileDate = sellerReport.CreateDate;
            _report.DocName = "Универсальный передаточный документ";
            _report.Function = sellerReport.Function;
            _report.SellerInvoiceNumber = sellerReport.DocNumber;
            _report.SellerInvoiceDate = sellerReport.DocDate;

            reportControl.SetDefaults();

            _report.EdoProgramVersion = edoProgramVersion;
            _report.OnAllPropertyChanged();
        }
    }
}
