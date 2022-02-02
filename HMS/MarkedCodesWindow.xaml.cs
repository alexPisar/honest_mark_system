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
    /// Логика взаимодействия для MarkedCodesWindow.xaml
    /// </summary>
    public partial class MarkedCodesWindow : Window
    {
        public MarkedCodesWindow()
        {
            InitializeComponent();
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            ((Models.MarkedCodesModel)DataContext).OnPropertyChanged("SelectedItems");

            if(((Models.MarkedCodesModel)DataContext).SelectedItems.Count == 0)
            {
                MessageBox.Show("Не выбраны коды маркировки.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
