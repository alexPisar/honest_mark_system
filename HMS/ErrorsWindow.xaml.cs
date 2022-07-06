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

namespace HonestMarkSystem
{
    /// <summary>
    /// Логика взаимодействия для ErrorsWindow.xaml
    /// </summary>
    public partial class ErrorsWindow : Window
    {
        public ErrorsWindow(string mainText, List<string> errors = null)
        {
            InitializeComponent();
            errorsControl.MainText = mainText;
            errorsControl.Errors = errors;
            errorsControl.OkButtonClick += (object sender, RoutedEventArgs e) => { Close(); };
            errorsControl.AfterSuccessSaveErrorFile += (object sender, RoutedEventArgs e) => 
            {
                var loadWindow = new LoadWindow();
                loadWindow.GetLoadContext().SetSuccessFullLoad("Файл ошибок успешно сохранён.");
                loadWindow.ShowDialog();
            };
        }
    }
}
