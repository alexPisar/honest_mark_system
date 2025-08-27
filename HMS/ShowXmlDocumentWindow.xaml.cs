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
    /// Interaction logic for ShowXmlDocumentWindow.xaml
    /// </summary>
    public partial class ShowXmlDocumentWindow : Window
    {
        private Models.ConsignorOrganization _selectedMyOrganization { get; set; }
        private List<Models.TreeListGoodInfo> _items;

        public ShowXmlDocumentWindow(Models.ConsignorOrganization selectedMyOrganization)
        {
            _selectedMyOrganization = selectedMyOrganization;
            _items = new List<Models.TreeListGoodInfo>();
            InitializeComponent();
        }

        public void InitializeDocument(string fileVersionFormat, string filePath)
        {
            var xmlDocument = new System.Xml.XmlDocument();
            xmlDocument.Load(filePath);
            var docContent = Encoding.GetEncoding(1251).GetBytes(xmlDocument.OuterXml);

            var reporterDll = new Reporter.ReporterDll();
            Reporter.IReport report;
            List<Reporter.Entities.Product> products;

            if (fileVersionFormat == "utd970_05_03_01")
            {
                report = reporterDll.ParseDocument<Reporter.Reports.UniversalTransferSellerDocumentUtd970>(docContent);
                products = (report as Reporter.Reports.UniversalTransferSellerDocumentUtd970).Products;
            }
            else
            {
                report = reporterDll.ParseDocument<Reporter.Reports.UniversalTransferSellerDocument>(docContent);
                products = (report as Reporter.Reports.UniversalTransferSellerDocument).Products;
            }

            SetMarkedItems(products);
        }

        public void SetItems()
        {
            treeList.Nodes.Clear();
            foreach (var item in _items)
            {
                var productNode = new DevExpress.Xpf.Grid.TreeListNode(item);

                foreach(var child in item.Children)
                {
                    var childNode = new DevExpress.Xpf.Grid.TreeListNode(child);
                    productNode.Nodes.Add(childNode);
                }
                treeList.Nodes.Add(productNode);
            }

            DocumentDetailsDataGrid.RefreshData();
        }

        private void SetMarkedItems(List<Reporter.Entities.Product> products)
        {
            var honestMarkSystem = _selectedMyOrganization?.HonestMarkSystem;

            if (honestMarkSystem == null)
                throw new Exception("Не удалось авторизоваться в Честном знаке.");

            foreach (var product in products)
            {
                List<KeyValuePair<string, string>> productMarkedCodes = new List<KeyValuePair<string, string>>();

                var transportCodes = product.TransportPackingIdentificationCode?.Distinct() ?? new List<string>();

                if(transportCodes.Count() > 0)
                    productMarkedCodes = honestMarkSystem.GetCodesByThePiece(transportCodes, productMarkedCodes);

                if(product.MarkedCodes != null && product.MarkedCodes.Count > 0)
                {
                    if (product.MarkedCodes.All(m => m?.Length == 31))
                        productMarkedCodes = honestMarkSystem.GetCodesByThePiece(product.MarkedCodes, productMarkedCodes, false);
                    else
                        productMarkedCodes = honestMarkSystem.GetCodesByThePiece(product.MarkedCodes, productMarkedCodes);
                }

                SetMarkedItem(product, productMarkedCodes?.Select(p => p.Key) ?? new List<string>());
            }
        }

        private void SetMarkedItem(Reporter.Entities.Product product, IEnumerable<string> productMarkedCodes)
        {
            var productGoodInfo = new Models.TreeListGoodInfo
            {
                Name = product.Description,
                Quantity = Convert.ToInt32(product.Quantity),
                BarCode = product.BarCode,
                NotMarked = productMarkedCodes.Count() == 0,
                NotAllDocumentsMarked = productMarkedCodes.Count() > 0 && productMarkedCodes.Count() < product.Quantity
            };

            foreach (var productMarkedCode in productMarkedCodes)
            {
                var productMarkedCodeInfo = new Models.TreeListGoodInfo
                {
                    Name = productMarkedCode,
                    Quantity = 1
                };

                productGoodInfo.Children.Add(productMarkedCodeInfo);
            }

            _items.Add(productGoodInfo);
        }
    }
}
