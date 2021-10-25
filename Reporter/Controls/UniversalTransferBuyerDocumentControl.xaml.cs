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
    /// Логика взаимодействия для UniversalTransferBuyerDocumentControl.xaml
    /// </summary>
    public partial class UniversalTransferBuyerDocumentControl : Base.ReportControlBase
    {
        public UniversalTransferBuyerDocumentControl()
        {
            InitializeComponent();
            DataContext = new UniversalTransferBuyerDocument();
        }

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
    }
}
