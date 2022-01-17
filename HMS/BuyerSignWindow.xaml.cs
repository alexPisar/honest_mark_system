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
    /// Логика взаимодействия для BuyerSignWindow.xaml
    /// </summary>
    public partial class BuyerSignWindow : Window
    {
        private string _prefixBuyerFileName = "ON_NSCHFDOPPOK";
        private string _prefixSellerFileName = "ON_NSCHFDOPPR";
        private string _filePath;
        private byte[] _docSellerContent;
        private CryptoUtil _cryptoUtil;
        private UtilityLog _log = UtilityLog.GetInstance();

        public BuyerSignWindow(CryptoUtil cryptoUtil, string filePath)
        {
            InitializeComponent();
            reportControl.OnCancelButtonClick += CancelButtonClick;
            reportControl.OnChangeButtonClick += SendButtonClick;
            reportControl.OnSaveButtonClick += SaveButtonClick;
            _cryptoUtil = cryptoUtil;
            _filePath = filePath;
        }

        public void OnAllPropertyChanged()
        {
            reportControl.OnAllPropertyCnanged();
        }

        public UniversalTransferBuyerDocument Report
        {
            get {
                return (UniversalTransferBuyerDocument)reportControl.Report;
            }
        }

        public byte[] DocSellerContent
        {
            get {
                if (_docSellerContent == null)
                    _docSellerContent = System.IO.File.ReadAllBytes(_filePath);

                return _docSellerContent;
            }
        }

        public byte[] SellerSignature { get; set; }

        private void CancelButtonClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void SendButtonClick(object sender, RoutedEventArgs e)
        {
            bool validationResult = reportControl.ValidateReport();

            if (validationResult)
            {
                Report.Signature = GetSignatureStringForReport();
                Report.DateReceive = DateTime.Now;

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

        private void SaveButtonClick(object sender, RoutedEventArgs e)
        {
            var changePathDialog = new Microsoft.Win32.SaveFileDialog();
            changePathDialog.Title = "Сохранение файла";
            changePathDialog.Filter = "XML Files|*.xml";

            try
            {
                Report.Signature = GetSignatureStringForReport();
                Report.DateReceive = DateTime.Now;
                changePathDialog.FileName = Report.FileName;

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
            catch (Exception ex)
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

        private string GetSignatureStringForReport()
        {
            var signatureAsBase64 = Convert.ToBase64String(SellerSignature);
            return signatureAsBase64;
        }

        public void SetDefaultParameters(string subject, DataContextManagementUnit.DataAccess.Contexts.Abt.DocEdoPurchasing dataBaseObject)
        {
            Report.CreateBuyerFileDate = DateTime.Now;

            var firstMiddleName = _cryptoUtil.ParseCertAttribute(subject, "G");
            Report.SignerEntity = new Reporter.Entities.IndividualEntity()
            {
                Inn = _cryptoUtil.ParseCertAttribute(subject, "ИНН").TrimStart('0'),
                Surname = _cryptoUtil.ParseCertAttribute(subject, "SN"),
                Name = firstMiddleName.IndexOf(" ") > 0 ? firstMiddleName.Substring(0, firstMiddleName.IndexOf(" ")) : string.Empty,
                Patronymic = firstMiddleName.IndexOf(" ") >= 0 && firstMiddleName.Length > firstMiddleName.IndexOf(" ") + 1 ? firstMiddleName.Substring(firstMiddleName.IndexOf(" ") + 1) : string.Empty
            };

            var orgInn = _cryptoUtil.GetCertificateAttributeValueByOid("1.2.643.100.4");
            var orgName = _cryptoUtil.ParseCertAttribute(subject, "CN").Replace("\"\"", "\"").Replace("\"\"", "\"").TrimStart('"');

            Report.BasisOfAuthority = _cryptoUtil.ParseCertAttribute(subject, "T");
            Report.ScopeOfAuthority = Reporter.Enums.ScopeOfAuthorityEnum.PersonWhoMadeOperation;
            Report.SignerStatus = Reporter.Enums.SignerStatusEnum.Individual;
            Report.AcceptResult = Reporter.Enums.AcceptResultEnum.GoodsAcceptedWithoutDiscrepancy;
            Report.FinSubjectCreator = $"{orgName}, ИНН: {orgInn}";

            Report.SellerFileId = dataBaseObject.FileName;
            Report.EdoProviderOrgName = dataBaseObject.SenderEdoOrgName;
            Report.ProviderInn = dataBaseObject.SenderEdoOrgInn;
            Report.EdoId = dataBaseObject.SenderEdoOrgId;
            Report.SenderEdoId = dataBaseObject.ReceiverEdoId;
            Report.ReceiverEdoId = dataBaseObject.SenderEdoId;

            if(dataBaseObject.FileName.StartsWith($"{_prefixSellerFileName}MARK"))
                Report.FileName = $"{_prefixBuyerFileName}MARK_{Report.ReceiverEdoId}_{Report.SenderEdoId}_{DateTime.Now.ToString("yyyyMMdd")}_{Guid.NewGuid().ToString()}";
            else if(dataBaseObject.FileName.StartsWith($"{_prefixSellerFileName}PROS"))
                Report.FileName = $"{_prefixBuyerFileName}PROS_{Report.ReceiverEdoId}_{Report.SenderEdoId}_{DateTime.Now.ToString("yyyyMMdd")}_{Guid.NewGuid().ToString()}";
            else
                Report.FileName = $"{_prefixBuyerFileName}_{Report.ReceiverEdoId}_{Report.SenderEdoId}_{DateTime.Now.ToString("yyyyMMdd")}_{Guid.NewGuid().ToString()}";

            var reporterDll = new Reporter.ReporterDll();
            var sellerReport = reporterDll.ParseDocument<UniversalTransferSellerDocument>(DocSellerContent);

            Report.CreateSellerFileDate = sellerReport.CreateDate;
            Report.DocName = sellerReport.DocName;
            Report.Function = sellerReport.Function;
            Report.SellerInvoiceNumber = sellerReport.DocNumber;
            Report.SellerInvoiceDate = sellerReport.DocDate;

            reportControl.SetDefaults();

            Report.OnAllPropertyChanged();
        }
    }
}
