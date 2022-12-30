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
using HonestMarkSystem.Interfaces;

namespace HonestMarkSystem
{
    /// <summary>
    /// Логика взаимодействия для ReturnWindow.xaml
    /// </summary>
    public partial class ReturnWindow : Window
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

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            ((Models.ReturnModel)DataContext).SetDocuments(Number, Comment, DateFrom, DateTo, this);
        }

        public ReturnWindow()
        {
            InitializeComponent();
        }

        private void ReturnButton_Click(object sender, RoutedEventArgs e)
        {
            (DataContext as Models.ReturnModel).ReturnCodes();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void ReturnDocumentsDataGrid_SelectedItemChanged(object sender, DevExpress.Xpf.Grid.SelectedItemChangedEventArgs e)
        {
            (DataContext as Models.ReturnModel).OnPropertyChanged("SelectedItem");
            (DataContext as Models.ReturnModel).OnPropertyChanged("IsReturnButtonEnabled");
        }
    }
}
