using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;

namespace HonestMarkSystem.Models
{
    public class ShowMarkedCodesModel<T> : Interfaces.ITreeListView where T:DbContext
    {
        private Interfaces.IEdoDataBaseAdapter<T> _dataBaseAdapter;
        private DataContextManagementUnit.DataAccess.Contexts.Abt.DocEdoPurchasing _selectedDocument;
        private object _docJournal;

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

                    var node = new DevExpress.Xpf.Grid.TreeListNode(treeItemCode);
                    node.IsChecked = false;
                    detailNode.Nodes.Add(node);
                }
            }
        }

        public void ProcessingSelectedItems(IEnumerable<DevExpress.Xpf.Grid.TreeListNode> nodes)
        {
            var selectedNodes = nodes?
                .Where(n => !(n.Content as Implementations.TreeListGoodInfo).NotMarked)?
                .SelectMany(n => n.Nodes)
                .Where(n => n.IsChecked == true)?? new List<DevExpress.Xpf.Grid.TreeListNode>();

            if (selectedNodes.Count() == 0)
                return;

            var selectedItems = selectedNodes.Select(n => n.Content as Implementations.TreeListGoodInfo);
        }
    }
}
