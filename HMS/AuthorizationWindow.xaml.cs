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

                if (Config.GetInstance().ProxyEnabled)
                {
                    var webProxy = new System.Net.WebProxy();

                    webProxy.Address = new Uri("http://" + Config.GetInstance().ProxyAddress);
                    webProxy.Credentials = new System.Net.NetworkCredential(Config.GetInstance().ProxyUserName,
                        Config.GetInstance().ProxyUserPassword);

                    client.Proxy = webProxy;
                }

                var certs = new CryptoUtil().GetPersonalCertificates().OrderByDescending(c => c.NotBefore);
                var cryptoUtil = new CryptoUtil(client);

                Interfaces.IEdoDataBaseAdapter<AbtDbContext> dataBaseAdapter = new Implementations.DiadocEdoToDataBase();

                ((Config)DataContext).GenerateParametersForPassword();
                ((Config)DataContext).SetDataBasePassword(accountPassword.Text);
                ((Config)DataContext).Save((Config)DataContext, Config.ConfFileName);
                dataBaseAdapter.InitializeContext();

                var refOrgs = dataBaseAdapter.GetMyOrganisations(Config.GetInstance().DataBaseUser).Cast<RefCustomer>();

                var myOrganizations = new List<Models.ConsignorOrganization>();
                var mainModel = new Models.MainViewModel(dataBaseAdapter, myOrganizations);

                foreach (var refOrg in refOrgs)
                {
                    var selectedCert = certs?.FirstOrDefault(c =>
                    cryptoUtil.GetOrgInnFromCertificate(c) == refOrg.Inn
                    && cryptoUtil.IsCertificateValid(c) && c.NotAfter > DateTime.Now);

                    if (selectedCert == null)
                    {
                        _log.Log($"Не найден сертификат организации с ИНН {refOrg.Inn}");
                        continue;
                    }

                    WebSystems.Systems.HonestMarkSystem markSystem;
                    IEdoSystem edoSystem;

                    if (AuthentificationByCert(selectedCert, out markSystem, out edoSystem))
                    {
                        var orgName = cryptoUtil.ParseCertAttribute(selectedCert.Subject, "CN");

                        var organization = new Models.ConsignorOrganization
                        {
                            Certificate = selectedCert,
                            HonestMarkSystem = markSystem,
                            CryptoUtil = new CryptoUtil(selectedCert),
                            EdoSystem = edoSystem,
                            OrgInn = refOrg.Inn,
                            OrgName = orgName
                        };

                        mainModel.SaveOrgData(organization);

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

        private bool AuthentificationByCert(X509Certificate2 certificate, out WebSystems.Systems.HonestMarkSystem markSystem, out IEdoSystem edoSystem)
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
                bool result = edoSystem.Authorization();

                try
                {
                    //if(((Config)DataContext).ConsignorInn != "9652306541")
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
