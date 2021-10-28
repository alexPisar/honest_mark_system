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
    /// Логика взаимодействия для BuyerSignWindow.xaml
    /// </summary>
    public partial class BuyerSignWindow : Window
    {
        public BuyerSignWindow()
        {
            InitializeComponent();
            reportControl.OnCancelButtonClick += CancelButtonClick;
            reportControl.OnChangeButtonClick += SendButtonClick;
        }

        public void OnAllPropertyChanged()
        {
            reportControl.OnAllPropertyCnanged();
        }

        public UniversalTransferBuyerDocument Report
        {
            get {
                return (UniversalTransferBuyerDocument)reportControl.Report;
            }
        }

        private void CancelButtonClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void SendButtonClick(object sender, RoutedEventArgs e)
        {
            bool validationResult = reportControl.ValidateReport();

            if (validationResult)
            {
                DialogResult = true;
                Close();
            }
            else
            {
                var errorsWindow = new ErrorsWindow("Ошибка валидации", reportControl.ErrorsList);
                errorsWindow.ShowDialog();
            }
        }
    }
}
