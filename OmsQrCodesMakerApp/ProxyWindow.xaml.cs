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
    /// Interaction logic for ProxyWindow.xaml
    /// </summary>
    public partial class ProxyWindow : Window
    {
        public ProxyWindow(string proxyAddress, string baseUrlAddress)
        {
            InitializeComponent();
            proxyControl.Initialize(proxyAddress, baseUrlAddress);
            proxyControl.CancelButtonClick += (object s, RoutedEventArgs e) => { this.DialogResult = false; this.Close(); };
            proxyControl.CheckProxySuccess += (Control p) => { this.DialogResult = true; this.Close(); };
        }

        public string ProxyLogin
        {
            get => proxyControl?.ProxyLogin;
            set => proxyControl.ProxyLogin = value;
        }
        public string ProxyPassword
        {
            get => proxyControl?.ProxyPassword;
            set => proxyControl.ProxyPassword = value;
        }
    }
}
