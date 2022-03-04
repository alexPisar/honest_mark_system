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
    /// Логика взаимодействия для ConfirmRevokeWindow.xaml
    /// </summary>
    public partial class ConfirmRevokeWindow : Window
    {
        private RevokeRequestDialogResult _result;

        public RevokeRequestDialogResult Result => _result;
        public ClarificationCorrectionRequestDocument Report => (ClarificationCorrectionRequestDocument)DataContext;

        public ConfirmRevokeWindow()
        {
            InitializeComponent();
            revokeRadioButton.IsChecked = true;
            rejectRevokePanel.IsEnabled = false;

            _result = RevokeRequestDialogResult.None;
            DataContext = new ClarificationCorrectionRequestDocument();
        }

        private void RevokeRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            rejectRevokePanel.IsEnabled = false;
        }

        private void RejectRevokeRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            rejectRevokePanel.IsEnabled = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            _result = RevokeRequestDialogResult.None;
            Close();
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            if (revokeRadioButton.IsChecked == true)
                _result = RevokeRequestDialogResult.Revoke;
            else if (rejectRevokeRadioButton.IsChecked == true)
            {
                if (string.IsNullOrEmpty(((ClarificationCorrectionRequestDocument)DataContext).Text))
                {
                    MessageBox.Show("Не указана причина отказа.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                _result = RevokeRequestDialogResult.RejectRevoke;
            }

            Close();
        }
    }

    public enum RevokeRequestDialogResult
    {
        None,
        Revoke,
        RejectRevoke
    }
}
