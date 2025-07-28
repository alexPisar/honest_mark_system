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
    /// Interaction logic for SavePrintDataMatrixWindow.xaml
    /// </summary>
    public partial class SavePrintDataMatrixWindow : Window
    {
        public SavePrintDataMatrixWindow(string defaultPrefix = null)
        {
            InitializeComponent();
            PrefixTextEdit.EditValue = defaultPrefix;
        }

        public int? Index
        {
            get
            {
                if (IndexSpinEdit?.EditValue == null)
                    return null;

                return Convert.ToInt32(IndexSpinEdit.EditValue);
            }
        }
        public string Prefix => PrefixTextEdit?.EditValue as string;

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            if(string.IsNullOrEmpty(PrefixTextEdit?.EditValue as string))
            {
                DialogResult = false;
                DevExpress.Xpf.Core.DXMessageBox.Show("Не указан префикс!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            DialogResult = true;
            this.Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            this.Close();
        }
    }
}
