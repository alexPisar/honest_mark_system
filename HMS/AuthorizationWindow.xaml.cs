using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using DevExpress.Xpf.Ribbon;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using ConfigSet.Configs;
using UtilitesLibrary.Service;
using System.Security.Cryptography.X509Certificates;
using DataContextManagementUnit.DataAccess.Contexts.Abt;
using WebSystems;
using WebSystems.EdoSystems;

namespace HonestMarkSystem
{
    /// <summary>
    /// Логика взаимодействия для CertsChangeWindow.xaml
    /// </summary>
    public partial class AuthorizationWindow : DXRibbonWindow
    {
        private UtilityLog _log = UtilityLog.GetInstance();

        public AuthorizationWindow()
        {
            InitializeComponent();
            DataContext = Config.GetInstance();
            accountPassword.Text = ((Config)DataContext).GetDataBasePassword();
        }

        private void ChangeButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var client = new System.Net.WebClient();

                string proxyPassword = null;
                if (Config.GetInstance().ProxyEnabled)
                {
                    var finDbWebClient = WebSystems.WebClients.FinDbWebClient.GetInstance();
                    var appName = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;
                    var checkProxyBaseUrl = finDbWebClient.GetApplicationConfigParameter<string>(appName, "CheckProxyBaseUrl");

                    bool checkConnectResult = false;
                    if (!(string.IsNullOrEmpty(Config.GetInstance()?.ProxyUserName) || string.IsNullOrEmpty(Config.GetInstance()?.ProxyUserPassword)))
                    {
                        var proxyConnectObj = new ProxyConnect(Config.GetInstance().ProxyAddress, checkProxyBaseUrl);
                        checkConnectResult = proxyConnectObj.CheckProxyConnect(Config.GetInstance().ProxyUserName, Config.GetInstance().GetProxyPassword());
                    }

                    if (!checkConnectResult)
                    {
                        var proxyWindow = new ProxyWindow(Config.GetInstance().ProxyAddress, checkProxyBaseUrl);
                        proxyWindow.ProxyLogin = Config.GetInstance().ProxyUserName;
                        proxyWindow.ProxyPassword = Config.GetInstance().GetProxyPassword();
                        checkConnectResult = proxyWindow.ShowDialog() ?? false;

                        if (!checkConnectResult)
                            throw new Exception("Не удалось пройти авторизацию Прокси.");
                        else
                        {
                            Config.GetInstance().ProxyUserName = proxyWindow.ProxyLogin;
                            Config.GetInstance().SetProxyPassword(proxyWindow.ProxyPassword);
                        }
                    }

                    var webProxy = new System.Net.WebProxy();

                    webProxy.Address = new Uri("http://" + Config.GetInstance().ProxyAddress);
                    webProxy.Credentials = new System.Net.NetworkCredential(Config.GetInstance().ProxyUserName,
                        Config.GetInstance().GetProxyPassword());

                    client.Proxy = webProxy;
                    proxyPassword = Config.GetInstance().GetProxyPassword();
                }

                var certs = new CryptoUtil().GetPersonalCertificates().OrderByDescending(c => c.NotBefore);
                var cryptoUtil = new CryptoUtil(client);

                Interfaces.IEdoDataBaseAdapter<AbtDbContext> dataBaseAdapter = new Implementations.DiadocEdoToDataBase();

                ((Config)DataContext).GenerateParametersForPassword();
                ((Config)DataContext).SetDataBasePassword(accountPassword.Text);

                if (((Config)DataContext).ProxyEnabled)
                    ((Config)DataContext).SetProxyPassword(proxyPassword);

                ((Config)DataContext).Save((Config)DataContext, Config.ConfFileName);
                dataBaseAdapter.InitializeContext();

                var refOrgs = dataBaseAdapter.GetMyOrganisations<RefCustomer, IEnumerable<RefCustomer>>(Config.GetInstance().DataBaseUser);

                var myOrganizations = new List<Models.ConsignorOrganization>();
                var mainModel = new Models.MainViewModel(dataBaseAdapter, myOrganizations);

                foreach (var refOrg in refOrgs)
                {
                    X509Certificate2 selectedCert = null;

                    var refAuthoritySignDocuments = dataBaseAdapter.GetRefAuthoritySignDocumentsByCustomer(refOrg.Key.Id)
                        as RefAuthoritySignDocuments;

                    if(refAuthoritySignDocuments != null)
                        selectedCert = certs?.FirstOrDefault(c => 
                        cryptoUtil.ParseCertAttribute(c.Subject, "ИНН").TrimStart('0') == refAuthoritySignDocuments.Inn
                        && cryptoUtil.IsCertificateValid(c) && c.NotAfter > DateTime.Now);

                    if (selectedCert == null)
                    {
                        refAuthoritySignDocuments = null;
                        selectedCert = certs?.FirstOrDefault(c => cryptoUtil.GetOrgInnFromCertificate(c) == refOrg.Key.Inn
                        && cryptoUtil.IsCertificateValid(c) && c.NotAfter > DateTime.Now);
                    }

                    if (selectedCert == null)
                    {
                        _log.Log($"Не найден сертификат организации с ИНН {refOrg.Key.Inn}");
                        continue;
                    }

                    WebSystems.Systems.HonestMarkSystem markSystem;
                    IEdoSystem edoSystem;

                    if (AuthentificationByCert(selectedCert, refAuthoritySignDocuments != null ? refOrg.Key.Inn : null, out markSystem, out edoSystem))
                    {
                        var orgName = refOrg.Key.Name;

                        var organization = new Models.ConsignorOrganization
                        {
                            Certificate = selectedCert,
                            HonestMarkSystem = markSystem,
                            CryptoUtil = new CryptoUtil(selectedCert),
                            EdoSystem = edoSystem,
                            OrgInn = refOrg.Key.Inn,
                            OrgName = orgName,
                            ShipperOrgInns = refOrg.Value?.Where(r => !string.IsNullOrEmpty(r.Inn))?.Select(r => r.Inn)?.ToList() ?? new List<string>()
                        };

                        mainModel.SaveOrgData(organization);

                        if (organization.EdoSystem != null)
                            organization.EdoSystem.CurrentOrgInn = organization.OrgInn;

                        if (refAuthoritySignDocuments != null)
                        {
                            organization.EmchdId = refAuthoritySignDocuments.EmchdId;
                            organization.EmchdPersonSurname = refAuthoritySignDocuments.Surname;
                            organization.EmchdPersonName = refAuthoritySignDocuments.Name;
                            organization.EmchdPersonPatronymicSurname = refAuthoritySignDocuments.PatronymicSurname;
                            organization.EmchdPersonPosition = refAuthoritySignDocuments.Position;
                            organization.EmchdPersonInn = refAuthoritySignDocuments.Inn;
                            organization.EmchdBeginDate = refAuthoritySignDocuments.EmchdBeginDate;
                        }

                        myOrganizations.Add(organization);
                    }
                }

                //((Config)DataContext).Save((Config)DataContext, Config.ConfFileName);
                var mainWindow = new MainWindow();
                
                mainModel.Owner = mainWindow;

                mainWindow.DataContext = mainModel;
                mainWindow.Show();

                Close();
            }
            catch (Exception ex)
            {
                string errorMessage = _log.GetRecursiveInnerException(ex);
                _log.Log(errorMessage);

                var errorsWindow = new ErrorsWindow("Произошла ошибка.", new List<string>(new string[] { errorMessage }));
                errorsWindow.ShowDialog();
            }
        }

        private bool AuthentificationByCert(X509Certificate2 certificate, string emchdOrgInn, out WebSystems.Systems.HonestMarkSystem markSystem, out IEdoSystem edoSystem)
        {
            markSystem = null;
            edoSystem = null;

            try
            {
                if (certificate == null)
                {
                    System.Windows.MessageBox.Show(
                        "Не выбран сертификат.", "Ошибка", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);

                    return false;
                }

                edoSystem = new DiadocEdoSystem(certificate);

                if (!string.IsNullOrEmpty(emchdOrgInn))
                    edoSystem.CurrentOrgInn = emchdOrgInn;

                bool result = edoSystem.Authorization();

                try
                {
                    //if(((Config)DataContext).ConsignorInn != "9652306541")
                    if(!string.IsNullOrEmpty(emchdOrgInn))
                        markSystem = new WebSystems.Systems.HonestMarkSystem(certificate, emchdOrgInn);
                    else
                        markSystem = new WebSystems.Systems.HonestMarkSystem(certificate);

                    if (markSystem != null && !markSystem.Authorization())
                        throw new Exception("Авторизация в Честном знаке не была успешной");
                }
                catch(System.Net.WebException webEx)
                {
                    string errorMessage = _log.GetRecursiveInnerException(webEx);
                    _log.Log($"Произошла ошибка авторизации в Честном знаке на удалённом сервере: {errorMessage}");

                    var errorsWindow = new ErrorsWindow("Произошла ошибка авторизации в Честном знаке на удалённом сервере.", new List<string>(new string[] { errorMessage }));
                    errorsWindow.ShowDialog();
                    markSystem = null;
                }
                catch(Exception ex)
                {
                    string errorMessage = _log.GetRecursiveInnerException(ex);
                    _log.Log($"Произошла ошибка авторизации в Честном знаке: {errorMessage}");

                    var errorsWindow = new ErrorsWindow("Произошла ошибка авторизации в Честном знаке.", new List<string>(new string[] { errorMessage }));
                    errorsWindow.ShowDialog();
                    markSystem = null;
                }

                return result;
            }
            catch (System.Net.WebException webEx)
            {
                string errorMessage = _log.GetRecursiveInnerException(webEx);
                _log.Log($"Произошла ошибка на удалённом сервере: {errorMessage}");

                var errorsWindow = new ErrorsWindow("Произошла ошибка на удалённом сервере.", new List<string>(new string[] { errorMessage }));
                errorsWindow.ShowDialog();

                return false;
            }
            catch (Exception ex)
            {
                string errorMessage = _log.GetRecursiveInnerException(ex);
                _log.Log(errorMessage);

                var errorsWindow = new ErrorsWindow("Произошла ошибка.", new List<string>(new string[] { errorMessage }));
                errorsWindow.ShowDialog();

                return false;
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
