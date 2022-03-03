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
    /// Логика взаимодействия для RevokeWindow.xaml
    /// </summary>
    public partial class RevokeWindow : Window
    {
        public RevokeWindow()
        {
            InitializeComponent();
            DataContext = new RevokeDocument();
        }

        public RevokeDocument Report => (RevokeDocument)DataContext;

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(((RevokeDocument)DataContext).Text))
            {
                MessageBox.Show("Не указана причина аннулирования.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            DialogResult = true;
            Close();
        }
    }
}
