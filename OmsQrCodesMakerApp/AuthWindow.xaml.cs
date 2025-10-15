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

namespace OmsQrCodesMakerApp
{
    /// <summary>
    /// Логика взаимодействия для AuthWindow.xaml
    /// </summary>
    public partial class AuthWindow : Window
    {
        public AuthWindow()
        {
            InitializeComponent();
            DataContext = new Models.AuthModel();
            (DataContext as Models.AuthModel)?.Init();
        }

        private void CertificatesLookUpEdit_EditValueChanged(object sender, DevExpress.Xpf.Editors.EditValueChangedEventArgs e)
        {
            (DataContext as Models.AuthModel)?.SelectedCertificateChanged();
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            if ((DataContext as Models.AuthModel) == null)
                return;

            if (ConfigSet.Configs.Config.GetInstance().ProxyEnabled)
            {
                var finDbWebClient = WebSystems.WebClients.FinDbWebClient.GetInstance();
                var appName = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;
                var checkProxyBaseUrl = finDbWebClient.GetApplicationConfigParameter<string>(appName, "CheckProxyBaseUrl");

                bool checkConnectResult = false;
                if (!(string.IsNullOrEmpty(ConfigSet.Configs.Config.GetInstance()?.ProxyUserName) || string.IsNullOrEmpty(ConfigSet.Configs.Config.GetInstance()?.ProxyUserPassword)))
                {
                    var proxyConnectObj = new UtilitesLibrary.Service.ProxyConnect(ConfigSet.Configs.Config.GetInstance().ProxyAddress, checkProxyBaseUrl);
                    checkConnectResult = proxyConnectObj.CheckProxyConnect(ConfigSet.Configs.Config.GetInstance().ProxyUserName, ConfigSet.Configs.Config.GetInstance().GetProxyPassword());
                }

                if (!checkConnectResult)
                {
                    var proxyWindow = new ProxyWindow(ConfigSet.Configs.Config.GetInstance().ProxyAddress, checkProxyBaseUrl);
                    proxyWindow.ProxyLogin = ConfigSet.Configs.Config.GetInstance().ProxyUserName;
                    proxyWindow.ProxyPassword = ConfigSet.Configs.Config.GetInstance().GetProxyPassword();
                    checkConnectResult = proxyWindow.ShowDialog() ?? false;

                    if (!checkConnectResult)
                    {
                        DevExpress.Xpf.Core.DXMessageBox.Show("Не удалось пройти авторизацию Прокси.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                    else
                    {
                        ConfigSet.Configs.Config.GetInstance().ProxyUserName = proxyWindow.ProxyLogin;
                        ConfigSet.Configs.Config.GetInstance().SetProxyPassword(proxyWindow.ProxyPassword);
                        ConfigSet.Configs.Config.GetInstance().Save(ConfigSet.Configs.Config.GetInstance(), ConfigSet.Configs.Config.ConfFileName);
                    }
                }
            }

            var authModel = DataContext as Models.AuthModel;

            if(authModel.SelectedCertificate?.Certificate == null)
            {
                DevExpress.Xpf.Core.DXMessageBox.Show("Не выбран сертификат!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (authModel.SelectedOrganization == null)
            {
                DevExpress.Xpf.Core.DXMessageBox.Show("Не выбрана организация!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if(!WebSystems.WebClients.OrderManagementStationClient.GetInstance().Authorization(authModel.SelectedCertificate.Certificate,
                authModel.SelectedOrganization.OmsConnection, authModel.SelectedOrganization.OmsId, authModel.SelectedOrganization.Inn))
            {
                DevExpress.Xpf.Core.DXMessageBox.Show("Не удалось авторизоваться в СУЗ!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var mainWindow = new MainWindow();
            var mainModel = new Models.MainViewModel();
            mainModel.Refresh();
            mainWindow.DataContext = mainModel;

            mainWindow.Show();
            this.Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
