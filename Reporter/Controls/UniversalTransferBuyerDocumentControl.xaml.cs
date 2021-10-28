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
            signerOrgTabItem.IsSelected = true;
            DataContext = new UniversalTransferBuyerDocument();
            SignerIndividual = new IndividualEntity();
            SignerJuridicalEntity = new JuridicalEntity();

            signerJurTabItem.DataContext = SignerJuridicalEntity;
            signerIndividualTabItem.DataContext = SignerIndividual;
        }

        public EventHandler<RoutedEventArgs> OnCancelButtonClick;
        public EventHandler<RoutedEventArgs> OnChangeButtonClick;

        public IndividualEntity SignerIndividual { get; set; }
        public JuridicalEntity SignerJuridicalEntity { get; set; }

        public override IReport Report
        {
            get {

               if(signerJurTabItem.IsSelected)
                {
                    ((UniversalTransferBuyerDocument)DataContext).JuridicalEntity = SignerJuridicalEntity;
                    ((UniversalTransferBuyerDocument)DataContext).Individual = null;
                }
               else if (signerIndividualTabItem.IsSelected)
                {
                    ((UniversalTransferBuyerDocument)DataContext).JuridicalEntity = null;
                    ((UniversalTransferBuyerDocument)DataContext).Individual = SignerIndividual;
                }

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
    }
}
