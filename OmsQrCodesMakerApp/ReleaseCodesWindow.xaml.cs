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
using UtilitesLibrary.Service;

namespace OmsQrCodesMakerApp
{
    /// <summary>
    /// Логика взаимодействия для ReleaseCodesWindow.xaml
    /// </summary>
    public partial class ReleaseCodesWindow : Window
    {
        private WebSystems.WebClients.OrderManagementStationClient _omsClient;
        private UtilityLog _log = UtilityLog.GetInstance();

        public ReleaseCodesWindow(WebSystems.WebClients.OrderManagementStationClient omsClient, int maxValue, int editValue = 0)
        {
            InitializeComponent();
            _omsClient = omsClient;
            CodesSpinEdit.Maximum = maxValue;
            CodesSpinEdit.Value = editValue;
        }

        public ViewModels.OmsOrder Order { get; set; }
        public ViewModels.OmsProduct Product { get; set; }

        private void ReleaseButton_Click(object sender, RoutedEventArgs e)
        {
            var editValue = CodesSpinEdit.Value;
            if (editValue == 0 || editValue == null)
            {
                MessageBox.Show("Указано нулевое количество кодов!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            int quantity = editValue.Value;

            if (Order == null)
            {
                MessageBox.Show("Не указан заказ!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (Product == null)
            {
                MessageBox.Show("Не указан товар!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                WebSystems.Models.OMS.MarkedCodes markedCodes = null;

                try
                {
                    markedCodes = _omsClient.GetMarkedCodes(Order.OrderId, quantity, Product.Gtin);
                }
                catch (System.Net.WebException webEx)
                {
                    string errorMessage = _log.GetRecursiveInnerException(webEx);
                    _log.Log(errorMessage);

                    var errorsWindow = new ErrorsWindow("Произошла ошибка выпуска кодов на удалённом сервере.", new List<string>(new string[] { errorMessage }));
                    errorsWindow.ShowDialog();
                    DialogResult = false;
                    return;
                }

                if (markedCodes?.Codes == null || markedCodes.Codes.Length == 0)
                {
                    MessageBox.Show("Не удалось получить коды маркировки!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var savePathDialog = new Microsoft.Win32.SaveFileDialog();
                savePathDialog.Title = "Сохранение файла";
                savePathDialog.Filter = "CSV File|*.csv";
                savePathDialog.FileName = $"order_{Order.OrderId}_gtin_{Product.Gtin}_quantity_{quantity}";

                if (savePathDialog.ShowDialog() == true)
                {
                    using (var fileStream = new System.IO.FileStream(savePathDialog.FileName, System.IO.FileMode.Create))
                    {
                        using (var streamWriter = new System.IO.StreamWriter(fileStream))
                        {
                            foreach (var markedCode in markedCodes.Codes)
                                streamWriter.WriteLine(markedCode);
                        }
                    }
                    DialogResult = true;
                }
                else
                    DialogResult = false;
            }
            catch (Exception ex)
            {
                string errorMessage = _log.GetRecursiveInnerException(ex);
                _log.Log(errorMessage);

                var errorsWindow = new ErrorsWindow("Произошла ошибка.", new List<string>(new string[] { errorMessage }));
                errorsWindow.ShowDialog();
                DialogResult = false;
            }

            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
