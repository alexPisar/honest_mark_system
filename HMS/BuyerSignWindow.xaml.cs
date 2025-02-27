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
    public partial class BuyerSignWindow : BaseControls.BaseBuyerSignWindow
    {
        private string _prefixBuyerFileName = "ON_NSCHFDOPPOK";
        private string _prefixSellerFileName = "ON_NSCHFDOPPR";
        private CryptoUtil _cryptoUtil;
        private UtilityLog _log = UtilityLog.GetInstance();

        private UniversalTransferBuyerDocument _report => Report as UniversalTransferBuyerDocument;

        public BuyerSignWindow(CryptoUtil cryptoUtil, string filePath)
        {
            InitializeComponent();
            reportControl.OnCancelButtonClick += CancelButtonClick;
            reportControl.OnChangeButtonClick += SendButtonClick;
            reportControl.OnSaveButtonClick += SaveButtonClick;
            _cryptoUtil = cryptoUtil;
            _filePath = filePath;
        }

        public override void OnAllPropertyChanged()
        {
            reportControl.OnAllPropertyCnanged();
        }

        public override string FileName => _report?.FileName;

        public override bool IsMarked => _report?.FileName?.StartsWith("ON_NSCHFDOPPOKMARK") ?? false;

        public override Reporter.IReport Report
        {
            get {
                return (UniversalTransferBuyerDocument)reportControl.Report;
            }
        }

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
                _report.Signature = GetSignatureStringForReport();
                _report.DateReceive = DateTime.Now;

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
                _report.Signature = GetSignatureStringForReport();
                _report.DateReceive = DateTime.Now;
                changePathDialog.FileName = _report.FileName;

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

        public override void SetDefaultParameters(Models.ConsignorOrganization organization, string subject, DataContextManagementUnit.DataAccess.Contexts.Abt.DocEdoPurchasing dataBaseObject, string edoProgramVersion)
        {
            _report.CreateBuyerFileDate = DateTime.Now;

            if (!string.IsNullOrEmpty(organization.EmchdId))
            {
                _report.SignerEntity = new Reporter.Entities.IndividualEntity()
                {
                    Inn = organization.EmchdPersonInn,
                    Surname = organization.EmchdPersonSurname,
                    Name = organization.EmchdPersonName,
                    Patronymic = organization.EmchdPersonPatronymicSurname
                };
                _report.BasisOfAuthority = "Доверенность";
            }
            else
            {
                var firstMiddleName = _cryptoUtil.ParseCertAttribute(subject, "G");
                _report.SignerEntity = new Reporter.Entities.IndividualEntity()
                {
                    Inn = _cryptoUtil.ParseCertAttribute(subject, "ИНН").TrimStart('0'),
                    Surname = _cryptoUtil.ParseCertAttribute(subject, "SN"),
                    Name = firstMiddleName.IndexOf(" ") > 0 ? firstMiddleName.Substring(0, firstMiddleName.IndexOf(" ")) : string.Empty,
                    Patronymic = firstMiddleName.IndexOf(" ") >= 0 && firstMiddleName.Length > firstMiddleName.IndexOf(" ") + 1 ? firstMiddleName.Substring(firstMiddleName.IndexOf(" ") + 1) : string.Empty
                };
                _report.BasisOfAuthority = _cryptoUtil.ParseCertAttribute(subject, "T");
            }

            var orgInn = organization.OrgInn;
            var orgName = organization.OrgName;

            if (string.IsNullOrEmpty(orgInn))
                orgInn = ((Reporter.Entities.IndividualEntity)_report.SignerEntity).Inn;

            _report.ScopeOfAuthority = Reporter.Enums.ScopeOfAuthorityEnum.PersonWhoMadeOperation;
            _report.SignerStatus = Reporter.Enums.SignerStatusEnum.Individual;
            _report.AcceptResult = Reporter.Enums.AcceptResultEnum.GoodsAcceptedWithoutDiscrepancy;
            _report.FinSubjectCreator = $"{orgName}, ИНН: {orgInn}";

            _report.SellerFileId = dataBaseObject.FileName;
            _report.EdoProviderOrgName = dataBaseObject.SenderEdoOrgName;
            _report.ProviderInn = dataBaseObject.SenderEdoOrgInn;
            _report.EdoId = dataBaseObject.SenderEdoOrgId;
            _report.SenderEdoId = dataBaseObject.ReceiverEdoId;
            _report.ReceiverEdoId = dataBaseObject.SenderEdoId;

            if(dataBaseObject.FileName.StartsWith($"{_prefixSellerFileName}MARK"))
                _report.FileName = $"{_prefixBuyerFileName}MARK_{_report.ReceiverEdoId}_{_report.SenderEdoId}_{DateTime.Now.ToString("yyyyMMdd")}_{Guid.NewGuid().ToString()}";
            else if(dataBaseObject.FileName.StartsWith($"{_prefixSellerFileName}PROS"))
                _report.FileName = $"{_prefixBuyerFileName}PROS_{_report.ReceiverEdoId}_{_report.SenderEdoId}_{DateTime.Now.ToString("yyyyMMdd")}_{Guid.NewGuid().ToString()}";
            else
                _report.FileName = $"{_prefixBuyerFileName}_{_report.ReceiverEdoId}_{_report.SenderEdoId}_{DateTime.Now.ToString("yyyyMMdd")}_{Guid.NewGuid().ToString()}";

            var reporterDll = new Reporter.ReporterDll();
            var sellerReport = reporterDll.ParseDocument<UniversalTransferSellerDocument>(DocSellerContent);

            _report.CreateSellerFileDate = sellerReport.CreateDate;
            _report.DocName = sellerReport.DocName;
            _report.Function = sellerReport.Function;
            _report.SellerInvoiceNumber = sellerReport.DocNumber;
            _report.SellerInvoiceDate = sellerReport.DocDate;

            reportControl.SetDefaults();
            _report.EdoProgramVersion = edoProgramVersion;

            _report.OnAllPropertyChanged();
        }
    }
}
