using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UtilitesLibrary.ModelBase;
using DataContextManagementUnit.DataAccess.Contexts.Abt;

namespace HonestMarkSystem.Models
{
    public class PurchasingDocumentsModel : ListViewModel<DocJournal>
    {
        private IEnumerable<DocJournal> _allDocs;
        public PurchasingDocumentsModel(IEnumerable<DocJournal> documents)
        {
            _allDocs = documents;
        }

        public void SetChangedDocument(decimal idDoc)
        {
            ItemsList = new System.Collections.ObjectModel.ObservableCollection<DocJournal>(_allDocs.Where(d => d.Id == idDoc));
            SelectedItem = ItemsList.FirstOrDefault();
            OnPropertyChanged("SelectedItem");
        }

        public void SetDocuments(string code, string comment, DateTime? dateFrom, DateTime? dateTo)
        {
            var items = _allDocs;

            if (!string.IsNullOrEmpty(code))
                items = items.Where(i => i.Code.Contains(code));

            if (!string.IsNullOrEmpty(comment))
                items = items.Where(i => i.Comment?.Contains(comment) ?? false);

            if(dateFrom != null)
                items = items.Where(i => i.DocDatetime >= dateFrom);

            if (dateTo != null)
                items = items.Where(i => i.DocDatetime <= dateTo);

            ItemsList = new System.Collections.ObjectModel.ObservableCollection<DocJournal>(items);
            SelectedItem = null;
            OnPropertyChanged("SelectedItem");
            OnPropertyChanged("ItemsList");
        }
    }
}
