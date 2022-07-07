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
    /// Логика взаимодействия для ErrorsControl.xaml
    /// </summary>
    public partial class ErrorsControl : UserControl
    {
        private List<string> _errors;

        public EventHandler<RoutedEventArgs> OkButtonClick { get; set; }
        public EventHandler<RoutedEventArgs> AfterSuccessSaveErrorFile { get; set; }

        public string MainText
        {
            get {
                return (string)mainLabel.Content;
            }
            set {
                mainLabel.Content = value;
            }
        }

        public List<string> Errors
        {
            get {
                return _errors;
            }
            set {
                if (value != null)
                {
                    string errorText = string.Join("\n", value);
                    textContent.Text = errorText;
                }
                else
                {
                    Details.Visibility = Visibility.Collapsed;
                    Height = Height - Details.Height;
                }

                _errors = value;
            }
        }

        public ErrorsControl()
        {
            InitializeComponent();
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            OkButtonClick?.Invoke(sender, e);
        }

        private void SaveErrorsButton_Click(object sender, RoutedEventArgs e)
        {
            var saveFileDialog = new Microsoft.Win32.SaveFileDialog();
            saveFileDialog.Title = "Сохранение файла";
            saveFileDialog.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
            saveFileDialog.FileName = "Errors.txt";

            var errStr = MainText + "\r\n" + string.Join("\r\n", _errors);
            var errBytes = Encoding.UTF8.GetBytes(errStr);

            if(saveFileDialog.ShowDialog() == true)
            {
                System.IO.File.WriteAllBytes(saveFileDialog.FileName, errBytes);
                AfterSuccessSaveErrorFile?.Invoke(sender, e);
            }
        }
    }
}
