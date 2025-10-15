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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace UtilitesLibrary.Controls
{
    /// <summary>
    /// Interaction logic for ProxyControl.xaml
    /// </summary>
    public partial class ProxyControl : UserControl
    {
        private Service.ProxyConnect _proxyConnectObj;

        public ProxyControl()
        {
            InitializeComponent();
        }

        public ProxyControl(string proxyAddress, string baseUrlAddress)
        {
            InitializeComponent();
            HeaderLabel.Content = HeaderLabel.Content + proxyAddress;
            _proxyConnectObj = new Service.ProxyConnect(proxyAddress, baseUrlAddress);
        }

        public void Initialize(string proxyAddress, string baseUrlAddress)
        {
            HeaderLabel.Content = HeaderLabel.Content + proxyAddress;
            _proxyConnectObj = new Service.ProxyConnect(proxyAddress, baseUrlAddress);
        }

        public string ProxyLogin
        {
            get => LoginTextBox?.Text;
            set => LoginTextBox.Text = value;
        }
        public string ProxyPassword
        {
            get => PasswordTextBox?.Password;
            set => PasswordTextBox.Password = value;
        }

        public Action<Control> CheckProxySuccess { get; set; }
        public EventHandler<RoutedEventArgs> CancelButtonClick { get; set; }

        private void EnterButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(ProxyLogin))
            {
                MessageBox.Show("Не указан логин прокси.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (string.IsNullOrEmpty(ProxyPassword))
            {
                MessageBox.Show("Не указан пароль прокси.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            bool successStatus = _proxyConnectObj.CheckProxyConnect(ProxyLogin, ProxyPassword);

            if (!successStatus)
            {
                MessageBox.Show("Авторизация прокси не была успешной.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            CheckProxySuccess?.Invoke(this);
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            CancelButtonClick?.Invoke(sender, e);
        }
    }
}
