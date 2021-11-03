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
using Reporter.Reports;
using Reporter.Entities;
using Reporter.Enums;

namespace Reporter.Controls
{
    /// <summary>
    /// Логика взаимодействия для UniversalTransferBuyerDocumentControl.xaml
    /// </summary>
    public partial class UniversalTransferBuyerDocumentControl : Base.ReportControlBase
    {
        public UniversalTransferBuyerDocumentControl()
        {
            InitializeComponent();
            DataContext = new UniversalTransferBuyerDocument();
        }

        public EventHandler<RoutedEventArgs> OnCancelButtonClick;
        public EventHandler<RoutedEventArgs> OnChangeButtonClick;
        public EventHandler<RoutedEventArgs> OnSaveButtonClick;

        public override IReport Report
        {
            get {

               return (UniversalTransferBuyerDocument)DataContext;
            }
        }

        public void OnAllPropertyCnanged()
        {
            ((UniversalTransferBuyerDocument)DataContext).OnAllPropertyChanged();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            OnCancelButtonClick?.Invoke(sender, e);
        }

        private void ChangeButton_Click(object sender, RoutedEventArgs e)
        {
            OnChangeButtonClick?.Invoke(sender, e);
        }

        public override void SetDefaults()
        {
            signerOrgTabItem.IsSelected = true;
            anotherPersonTabItem.IsSelected = true;
            orgRepresentativeTabItem.IsSelected = true;
            ((AnotherPerson)anotherPersonTabItem.DataContext).Item = orgRepresentativeTabItem.DataContext;
            ((UniversalTransferBuyerDocument)DataContext).OrganizationEmployeeOrAnotherPerson = anotherPersonTabItem.DataContext;

            receiverTabControl.Loaded += (object sender, RoutedEventArgs e) => {
                anotherPersonTabItem.IsSelected = true;
                orgRepresentativeTabItem.IsSelected = true;
            };
            ((UniversalTransferBuyerDocument)DataContext).OnPropertyChanged("OrganizationEmployeeOrAnotherPerson");
            ((UniversalTransferBuyerDocument)DataContext).OnPropertyChanged("OrganizationEmployeeOrAnotherPerson.Item");
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            OnSaveButtonClick?.Invoke(sender, e);
        }
    }
}
