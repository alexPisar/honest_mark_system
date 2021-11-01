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

namespace HonestMarkSystem
{
    /// <summary>
    /// Логика взаимодействия для CertsChangeWindow.xaml
    /// </summary>
    public partial class CertsChangeWindow : DXRibbonWindow
    {
        public CertsChangeWindow()
        {
            InitializeComponent();
            DataContext = new Models.CertsChangeViewModel();
        }

        private void ChangeButton_Click(object sender, RoutedEventArgs e)
        {
            if (((Models.CertsChangeViewModel)DataContext).Authentification())
            {
                var mainWindow = new MainWindow();
                var mainModel = new Models.MainViewModel(((Models.CertsChangeViewModel)DataContext).EdoSystem, new Implementations.EdoLiteToDataBase());

                var cryptoUtil = new UtilitesLibrary.Service.CryptoUtil();
                var orgName = cryptoUtil.ParseCertAttribute(((Models.CertsChangeViewModel)DataContext).SelectedItem.Subject, "CN");
                var orgInn = cryptoUtil.ParseCertAttribute(((Models.CertsChangeViewModel)DataContext).SelectedItem.Subject, "ИНН").Trim('0');
                mainModel.SaveOrgData(orgInn, orgName);

                mainWindow.DataContext = mainModel;
                mainWindow.Show();

                Close();
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
