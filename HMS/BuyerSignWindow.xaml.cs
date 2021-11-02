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

namespace HonestMarkSystem
{
    /// <summary>
    /// Логика взаимодействия для BuyerSignWindow.xaml
    /// </summary>
    public partial class BuyerSignWindow : Window
    {
        private string _prefixFileName = "ON_NSCHFDOPPOKMARK";

        public BuyerSignWindow()
        {
            InitializeComponent();
            reportControl.OnCancelButtonClick += CancelButtonClick;
            reportControl.OnChangeButtonClick += SendButtonClick;
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
                DialogResult = true;
                Close();
            }
            else
            {
                var errorsWindow = new ErrorsWindow("Ошибка валидации", reportControl.ErrorsList);
                errorsWindow.ShowDialog();
            }
        }

        public void SetDefaultParameters(string subject, DataContextManagementUnit.DataAccess.Contexts.Abt.DocEdoPurchasing dataBaseObject)
        {
            reportControl.SetDefaults();

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
            Report.FileName = $"{_prefixFileName}_{Report.ReceiverEdoId}_{Report.SenderEdoId}_{DateTime.Now.ToString("yyyyMMdd")}_{Guid.NewGuid().ToString()}";

            Report.OnAllPropertyChanged();
        }
    }
}
