using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using Reporter.Entities;

namespace HonestMarkSystem.Models
{
    public class ShowMarkedCodesModel<T> : Interfaces.ITreeListView where T:DbContext
    {
        private Interfaces.IEdoDataBaseAdapter<T> _dataBaseAdapter;
        private DataContextManagementUnit.DataAccess.Contexts.Abt.DocEdoPurchasing _selectedDocument;
        private object _docJournal;

        public EventHandler OnReturnSelectedCodesProcess;

        public ShowMarkedCodesModel(DataContextManagementUnit.DataAccess.Contexts.Abt.DocEdoPurchasing selectedDocument, Interfaces.IEdoDataBaseAdapter<T> dataBaseAdapter, object docJournal)
        {
            _dataBaseAdapter = dataBaseAdapter;
            _selectedDocument = selectedDocument;
            _docJournal = docJournal;
        }

        public void SetItems(DevExpress.Xpf.Grid.TreeListNodeCollection nodeCollection)
        {
            foreach(var detail in _selectedDocument.Details)
            {
                var treeItemDetail = new Implementations.TreeListGoodInfo();
                treeItemDetail.Name = detail.Description;
                treeItemDetail.IdGood = detail.IdGood;
                treeItemDetail.BarCode = detail.BarCode;
                treeItemDetail.IdDoc = _selectedDocument.IdDocJournal;
                treeItemDetail.Quantity = detail.Quantity;
                treeItemDetail.Price = detail.Price * detail.Quantity;
                treeItemDetail.TaxAmount = detail.TaxAmount;

                var markedCodes = _dataBaseAdapter.GetMarkedCodesByDocGoodId(_docJournal, detail.IdGood) ?? new List<string>();

                treeItemDetail.NotMarked = markedCodes.Count() == 0;
                treeItemDetail.NotAllMarked = markedCodes.Count() < detail.Quantity;

                var detailNode = new DevExpress.Xpf.Grid.TreeListNode(treeItemDetail);
                detailNode.IsChecked = false;

                if (treeItemDetail.NotMarked)
                {
                    detailNode.IsCheckBoxEnabled = false;
                }

                nodeCollection.Add(detailNode);

                foreach (var markedCode in markedCodes)
                {
                    var treeItemCode = new Implementations.TreeListGoodInfo();
                    treeItemCode.Name = markedCode;
                    treeItemCode.IdDoc = _selectedDocument.IdDocJournal;
                    treeItemCode.IdGood = detail.IdGood;
                    treeItemCode.BarCode = detail.BarCode;
                    treeItemCode.IsMarkedCode = true;
                    treeItemCode.Price = detail.Price;

                    var node = new DevExpress.Xpf.Grid.TreeListNode(treeItemCode);
                    node.IsChecked = false;
                    detailNode.Nodes.Add(node);
                }
            }
        }

        public void ProcessingSelectedItems(IEnumerable<DevExpress.Xpf.Grid.TreeListNode> nodes)
        {
            var parentNodes = (from node in nodes
                              where node?.Nodes?.Any(n => n.IsChecked == true) ?? false
                              select node).Distinct();

            if (parentNodes.Count() == 0)
                throw new Exception("Не выбраны коды маркировки.");

            Dictionary<decimal, Product> lines = new Dictionary<decimal, Product>();
            int i = 0;

            foreach(var parentNode in parentNodes)
            {
                var parentTreeListItem = parentNode.Content as Implementations.TreeListGoodInfo;

                if (parentTreeListItem.IdGood == null)
                    throw new Exception("В документе определены не все товары (ID_GOOD).");

                var items = parentNode.Nodes.Where(n => n.IsChecked == true);
                var quantity = items.Count();

                var product = new Product()
                {
                    Number = ++i,
                    Description = parentTreeListItem.Name,
                    UnitCode = "796",
                    Quantity = quantity,
                    Price = Math.Round(parentTreeListItem.Price ?? 0 / quantity, 2),
                    TaxAmount = Math.Round(parentTreeListItem.TaxAmount ?? 0, 2),
                    BarCode = parentTreeListItem.BarCode,
                    UnitName = "шт"
                };

                var refGood = _dataBaseAdapter.GetRefGoodById(parentTreeListItem.IdGood.Value) as DataContextManagementUnit.DataAccess.Contexts.Abt.RefGood;

                product.OriginCode = refGood?.Country?.NumCode?.ToString();
                product.OriginCountryName = refGood?.Country?.Name?.ToString();

                if(!string.IsNullOrEmpty(refGood?.CustomsNo))
                    product.CustomsDeclarationCode = refGood?.CustomsNo;

                if ((parentTreeListItem.TaxAmount ?? 0) == 0)
                    product.VatRate = 0;
                else
                    product.VatRate = 20;

                product.MarkedCodes = items?.Select(item => (item.Content as Implementations.TreeListGoodInfo)?.Name)?.Where(s => !string.IsNullOrEmpty(s))?.ToList();
                lines.Add(parentTreeListItem.IdGood.Value, product);
            }

            OnReturnSelectedCodesProcess?.Invoke(lines, new EventArgs());
        }
    }
}
