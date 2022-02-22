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
    /// Логика взаимодействия для RejectWindow.xaml
    /// </summary>
    public partial class RejectWindow : Window
    {
        public RejectWindow()
        {
            InitializeComponent();
            DataContext = new ClarificationCorrectionRequestDocument();
        }

        public ClarificationCorrectionRequestDocument Report => (ClarificationCorrectionRequestDocument)DataContext;

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            if(string.IsNullOrEmpty(((ClarificationCorrectionRequestDocument)DataContext).Text))
            {
                MessageBox.Show("Не указана причина отклонения.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            DialogResult = true;
            Close();
        }
    }
}
