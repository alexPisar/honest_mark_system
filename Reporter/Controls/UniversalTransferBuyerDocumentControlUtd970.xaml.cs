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

namespace Reporter.Controls
{
    /// <summary>
    /// Логика взаимодействия для UniversalTransferBuyerDocumentControlUtd970.xaml
    /// </summary>
    public partial class UniversalTransferBuyerDocumentControlUtd970 : Base.ReportControlBase
    {
        protected override string UrlXmlValidation => "XSD//ON_NSCHFDOPPOK_1_997_02_05_03_01.xsd";

        public UniversalTransferBuyerDocumentControlUtd970()
        {
            InitializeComponent();
            DataContext = new UniversalTransferBuyerDocumentUtd970();
        }

        public EventHandler<RoutedEventArgs> OnCancelButtonClick;
        public EventHandler<RoutedEventArgs> OnChangeButtonClick;
        public EventHandler<RoutedEventArgs> OnSaveButtonClick;

        public override IReport Report
        {
            get {

                return (UniversalTransferBuyerDocumentUtd970)DataContext;
            }
        }

        public void OnAllPropertyCnanged()
        {
            ((UniversalTransferBuyerDocumentUtd970)DataContext).OnAllPropertyChanged();
        }

        public override void SetDefaults()
        {
            if((DataContext as UniversalTransferBuyerDocumentUtd970)?.SignerInfo != null)
            {
                signerReportAddedControl.CurrentTabControl.Items.Clear();
                signerReportAddedControl.AddItem(1);

                var item = signerReportAddedControl.CurrentTabControl.Items[0] as ContentControl;
                var powerOfAttorneyTabControl = (item.Content as StackPanel).Children.Cast<object>().FirstOrDefault(r => r as Base.ReportSwitchTabControl != null) as Base.ReportSwitchTabControl;

                if ((DataContext as UniversalTransferBuyerDocumentUtd970)?.SignerInfo?.ElectronicPowerOfAttorney != null)
                {
                    var electronicPowerOfAttorneyTabItem = powerOfAttorneyTabControl.Items.Cast<Base.ReportSwitchTabItem>().FirstOrDefault(r => r.Header as string == "Эл. доверенность") as Base.ReportSwitchTabItem;
                    (DataContext as UniversalTransferBuyerDocumentUtd970).SignerInfo.PowerOfAttorney = (DataContext as UniversalTransferBuyerDocumentUtd970).SignerInfo.ElectronicPowerOfAttorney;
                    electronicPowerOfAttorneyTabItem.DataEntityObject = (DataContext as UniversalTransferBuyerDocumentUtd970).SignerInfo.ElectronicPowerOfAttorney;
                    electronicPowerOfAttorneyTabItem.IsSelected = true;

                    personAcceptedGoodsControl.CurrentTabControl.Items.Clear();
                    personAcceptedGoodsControl.AddItem(1);

                    var tabControl = (personAcceptedGoodsControl.CurrentTabControl.Items[0] as TabItem).Content as Base.ReportSwitchTabControl;
                    var organizationEmployeeTabItem = tabControl.Items.Cast<Base.ReportSwitchTabItem>().First(r => r.Header as string == "Работник организации покупателя");

                    (DataContext as UniversalTransferBuyerDocumentUtd970).OrganizationEmployeeOrAnotherPerson = new List<object>(
                        new object[]
                        {
                            new Entities.OrganizationEmployee
                            {
                                Surname = (DataContext as UniversalTransferBuyerDocumentUtd970).SignerInfo.Surname,
                                Name = (DataContext as UniversalTransferBuyerDocumentUtd970).SignerInfo.Name,
                                Patronymic = (DataContext as UniversalTransferBuyerDocumentUtd970).SignerInfo.Patronymic,
                                Position = (DataContext as UniversalTransferBuyerDocumentUtd970).SignerInfo.Position
                            }
                        });

                    personAcceptedGoodsControl.AddButton.IsEnabled = false;
                    personAcceptedGoodsControl.RemoveButton.IsEnabled = true;
                    organizationEmployeeTabItem.DataEntityObject = (DataContext as UniversalTransferBuyerDocumentUtd970).OrganizationEmployeeOrAnotherPerson.First();
                    organizationEmployeeTabItem.DataContext = organizationEmployeeTabItem.DataEntityObject;
                    personAcceptedGoodsControl.OnItemsChanged();
                    (personAcceptedGoodsControl.CurrentTabControl.Items[0] as TabItem).IsSelected = true;
                }
                else if ((DataContext as UniversalTransferBuyerDocumentUtd970)?.SignerInfo?.PaperPowerOfAttorney != null)
                {
                    var paperPowerOfAttorneyTabItem = powerOfAttorneyTabControl.Items.Cast<Base.ReportSwitchTabItem>().FirstOrDefault(r => r.Header as string == "Бум. доверенность") as Base.ReportSwitchTabItem;
                    (DataContext as UniversalTransferBuyerDocumentUtd970).SignerInfo.PowerOfAttorney = (DataContext as UniversalTransferBuyerDocumentUtd970).SignerInfo.PaperPowerOfAttorney;
                    paperPowerOfAttorneyTabItem.DataEntityObject = (DataContext as UniversalTransferBuyerDocumentUtd970).SignerInfo.PaperPowerOfAttorney;
                    paperPowerOfAttorneyTabItem.IsSelected = true;
                }

                signerReportAddedControl.AddButton.IsEnabled = false;
                signerReportAddedControl.RemoveButton.IsEnabled = true;
                (item as TabItem).IsSelected = true;
                
                (item.Content as FrameworkElement).DataContext = (DataContext as UniversalTransferBuyerDocumentUtd970).SignerInfo;
                signerReportAddedControl.OnItemsChanged();
            }
            OnAllPropertyCnanged();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            OnCancelButtonClick?.Invoke(sender, e);
        }

        private void ChangeButton_Click(object sender, RoutedEventArgs e)
        {
            OnChangeButtonClick?.Invoke(sender, e);
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            OnSaveButtonClick?.Invoke(sender, e);
        }
    }
}
