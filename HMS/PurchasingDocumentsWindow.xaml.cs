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
    /// Логика взаимодействия для PurchasingDocumentsWindow.xaml
    /// </summary>
    public partial class PurchasingDocumentsWindow : Window
    {
        public DateTime? DateTo
        {
            get {
                return dateToPicker.SelectedDate;
            }
            set {
                dateToPicker.SelectedDate = value;
            }
        }

        public DateTime? DateFrom
        {
            get {
                return dateFromPicker.SelectedDate;
            }
            set {
                dateFromPicker.SelectedDate = value;
            }
        }

        public string Number
        {
            get {
                return numberTextBox.Text;
            }
            set {
                numberTextBox.Text = value;
            }
        }

        public string Comment
        {
            get {
                return commentTextBox.Text;
            }
            set {
                commentTextBox.Text = value;
            }
        }

        public PurchasingDocumentsWindow()
        {
            InitializeComponent();
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            ((Models.PurchasingDocumentsModel)DataContext).SetDocuments(Number, Comment, DateFrom, DateTo, this);
        }

        private void ChangeButton_Click(object sender, RoutedEventArgs e)
        {
            if(((Models.PurchasingDocumentsModel)DataContext).SelectedItem == null)
            {
                MessageBox.Show(
                    "Не выбран документ.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
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
