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
    }
}
