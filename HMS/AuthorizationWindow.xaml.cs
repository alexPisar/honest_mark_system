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
                var certs = new CryptoUtil().GetPersonalCertificates();

                var selectedCert = certs?.FirstOrDefault(c =>
                new CryptoUtil().ParseCertAttribute(c.Subject, "ИНН").TrimStart('0') == ((Config)DataContext).ConsignorInn
                && c.NotAfter > DateTime.Now);

                if (selectedCert == null)
                    throw new Exception("Не найден сертификат по ИНН организации.");

                ((Config)DataContext).SetDataBasePassword(accountPassword.Text);
                var dataBaseAdapter = new Implementations.EdoLiteToDataBase();

                WebSystems.Systems.HonestMarkSystem markSystem;
                IEdoSystem edoSystem;

                if (AuthentificationByCert(selectedCert, out markSystem, out edoSystem))
                {
                    var mainWindow = new MainWindow();
                    var mainModel = new Models.MainViewModel(edoSystem, markSystem, dataBaseAdapter, new CryptoUtil(selectedCert));
                    mainModel.Owner = mainWindow;

                    var cryptoUtil = new CryptoUtil();
                    var orgName = cryptoUtil.ParseCertAttribute(selectedCert.Subject, "CN");
                    var orgInn = ((Config)DataContext).ConsignorInn;
                    mainModel.SaveOrgData(orgInn, orgName);
                    dataBaseAdapter.InitializeContext();
                    ((Config)DataContext).Save((Config)DataContext, Config.ConfFileName);

                    mainWindow.DataContext = mainModel;
                    mainWindow.Show();

                    Close();
                }
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

                edoSystem = new EdoLiteSystem(certificate);
                bool result = edoSystem.Authorization();

                markSystem = new WebSystems.Systems.HonestMarkSystem(certificate);
                result = result && markSystem.Authorization();

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
