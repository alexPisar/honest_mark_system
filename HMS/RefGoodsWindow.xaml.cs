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
    /// Логика взаимодействия для RefGoodsWindow.xaml
    /// </summary>
    public partial class RefGoodsWindow : Window
    {
        public RefGoodsWindow()
        {
            InitializeComponent();
        }

        private void ChangeButton_Click(object sender, RoutedEventArgs e)
        {
            if (((UtilitesLibrary.ModelBase.ListViewModel<DataContextManagementUnit.DataAccess.Contexts.Abt.RefGood>)DataContext)?.SelectedItem == null)
            {
                MessageBox.Show("Не выбран товар", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
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
