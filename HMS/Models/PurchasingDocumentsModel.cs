using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UtilitesLibrary.ModelBase;
using DataContextManagementUnit.DataAccess.Contexts.Abt;

namespace HonestMarkSystem.Models
{
    public class PurchasingDocumentsModel : ListViewModel<DocPurchasing>
    {
        public PurchasingDocumentsModel(IEnumerable<DocPurchasing> documents)
        {
            ItemsList = new System.Collections.ObjectModel.ObservableCollection<DocPurchasing>(documents);
            OnPropertyChanged("ItemsList");
        }

        public void SetChangedDocument(decimal idDoc)
        {
            SelectedItem = ItemsList.FirstOrDefault(d => d.Id == idDoc);
            OnPropertyChanged("SelectedItem");
        }
    }
}
