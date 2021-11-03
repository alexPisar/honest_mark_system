﻿using System;
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
        private UtilitesLibrary.Service.CryptoUtil _cryptoUtil;
        private UtilitesLibrary.Service.UtilityLog _log = UtilitesLibrary.Service.UtilityLog.GetInstance();

        public BuyerSignWindow(UtilitesLibrary.Service.CryptoUtil cryptoUtil, string filePath)
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
                var docSellerContent = System.IO.File.ReadAllBytes(_filePath);
                Report.Signature = GetSignatureStringForReport(docSellerContent);

                DialogResult = true;
                Close();
            }
            else
            {
                var errorsWindow = new ErrorsWindow("Ошибка валидации", reportControl.ErrorsList);
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
                var docSellerContent = System.IO.File.ReadAllBytes(_filePath);
                Report.Signature = GetSignatureStringForReport(docSellerContent);
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

        private string GetSignatureStringForReport(byte[] fileBytes)
        {
            var signature = _cryptoUtil.Sign(fileBytes, true);
            var signatureAsBase64 = Convert.ToBase64String(signature);
            return signatureAsBase64;
        }

        public void SetDefaultParameters(string subject, DataContextManagementUnit.DataAccess.Contexts.Abt.DocEdoPurchasing dataBaseObject)
        {
            reportControl.SetDefaults();

            Report.CreateBuyerFileDate = DateTime.Now;

            var cryptoUtil = new UtilitesLibrary.Service.CryptoUtil();
            var firstMiddleName = cryptoUtil.ParseCertAttribute(subject, "G");
            Report.SignerName = firstMiddleName.IndexOf(" ") > 0 ? firstMiddleName.Substring(0, firstMiddleName.IndexOf(" ")) : string.Empty;
            Report.SignerPatronymic = firstMiddleName.IndexOf(" ") >= 0 && firstMiddleName.Length > firstMiddleName.IndexOf(" ") + 1 ? firstMiddleName.Substring(firstMiddleName.IndexOf(" ") + 1) : string.Empty;
            Report.SignerSurname = cryptoUtil.ParseCertAttribute(subject, "SN");
            Report.SignerOrgName = cryptoUtil.ParseCertAttribute(subject, "CN").Replace("\"\"", "\"").Replace("\"\"", "\"").TrimStart('"');
            Report.JuridicalInn = cryptoUtil.ParseCertAttribute(subject, "ИНН").TrimStart('0');
            Report.SignerPosition = cryptoUtil.ParseCertAttribute(subject, "T");

            ((Reporter.Entities.OrganizationRepresentative)((Reporter.Entities.AnotherPerson)Report.OrganizationEmployeeOrAnotherPerson).Item).OrgName = Report.SignerOrgName;
            ((Reporter.Entities.OrganizationRepresentative)((Reporter.Entities.AnotherPerson)Report.OrganizationEmployeeOrAnotherPerson).Item).Position = Report.SignerPosition;
            ((Reporter.Entities.OrganizationRepresentative)((Reporter.Entities.AnotherPerson)Report.OrganizationEmployeeOrAnotherPerson).Item).Surname = Report.SignerSurname;
            ((Reporter.Entities.OrganizationRepresentative)((Reporter.Entities.AnotherPerson)Report.OrganizationEmployeeOrAnotherPerson).Item).Name = Report.SignerName;
            ((Reporter.Entities.OrganizationRepresentative)((Reporter.Entities.AnotherPerson)Report.OrganizationEmployeeOrAnotherPerson).Item).Patronymic = Report.SignerPatronymic;

            Report.BasisOfAuthority = "Должностные обязанности";
            Report.ScopeOfAuthority = Reporter.Enums.ScopeOfAuthorityEnum.PersonWhoResponsibleForRegistrationExecution;
            Report.SignerStatus = Reporter.Enums.SignerStatusEnum.Individual;
            Report.AcceptResult = Reporter.Enums.AcceptResultEnum.GoodsAcceptedWithoutDiscrepancy;
            Report.FinSubjectCreator = $"{Report.SignerOrgName}, ИНН: {Report.JuridicalInn}";

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
            var docSellerContent = System.IO.File.ReadAllBytes(_filePath);
            var sellerReport = reporterDll.ParseDocument<UniversalTransferSellerDocument>(docSellerContent);

            Report.CreateSellerFileDate = sellerReport.CreateDate;
            Report.DocName = sellerReport.DocName;
            Report.Function = sellerReport.Function;
            Report.SellerInvoiceNumber = sellerReport.DocNumber;
            Report.SellerInvoiceDate = sellerReport.CreateDate;

            Report.OnAllPropertyChanged();
        }
    }
}
