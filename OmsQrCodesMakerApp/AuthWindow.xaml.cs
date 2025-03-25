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

            var authModel = DataContext as Models.AuthModel;

            if(authModel.SelectedCertificate == null)
            {
                DevExpress.Xpf.Core.DXMessageBox.Show("Не выбран сертификат!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (authModel.SelectedOrganization == null)
            {
                DevExpress.Xpf.Core.DXMessageBox.Show("Не выбрана организация!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if(!WebSystems.WebClients.OrderManagementStationClient.GetInstance().Authorization(authModel.SelectedCertificate,
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
