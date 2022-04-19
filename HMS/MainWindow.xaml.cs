using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using DevExpress.Xpf.Ribbon;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace HonestMarkSystem
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : DXRibbonWindow
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void SelectedItemChanged(object sender, DevExpress.Xpf.Grid.SelectedItemChangedEventArgs e)
        {
            ((UtilitesLibrary.ModelBase.ListViewModel<DataContextManagementUnit.DataAccess.Contexts.Abt.DocEdoPurchasing>)DataContext).OnPropertyChanged("Details");
            ((UtilitesLibrary.ModelBase.ListViewModel<DataContextManagementUnit.DataAccess.Contexts.Abt.DocEdoPurchasing>)DataContext).OnPropertyChanged("IsRevokedDocument");
        }

        private void ButtonEditSettings_DefaultButtonClick(object sender, RoutedEventArgs e)
        {
            ((Interfaces.IEdoDocumentsView)DataContext).UpdateIdGood();
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            ((UtilitesLibrary.ModelBase.ViewModelBase)DataContext).Dispose();
        }
    }
}
