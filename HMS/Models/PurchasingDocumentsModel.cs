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

        public async void SetDocuments(string code, string comment, DateTime? dateFrom, DateTime? dateTo, System.Windows.Window owner = null)
        {
            var loadWindow = new LoadWindow("Идёт поиск...");

            if(owner != null)
                loadWindow.Owner = owner;

            loadWindow.Show();
            Exception exception = null;
            var loadContext = loadWindow.GetLoadContext();

            await Task.Run(() =>
            {
                try
                {
                    var items = _allDocs;

                    if (dateFrom != null)
                        items = items.Where(i => i.DocDatetime >= dateFrom);

                    if (dateTo != null)
                        items = items.Where(i => i.DocDatetime <= dateTo);

                    if (!string.IsNullOrEmpty(code))
                        items = items.Where(i => i.Code.Contains(code));

                    if (!string.IsNullOrEmpty(comment))
                        items = items.Where(i => i.Comment?.Contains(comment) ?? false);

                    ItemsList = new System.Collections.ObjectModel.ObservableCollection<DocJournal>(items);
                    SelectedItem = null;
                    OnPropertyChanged("SelectedItem");
                    OnPropertyChanged("ItemsList");
                    loadContext.SetSuccessFullLoad();
                }
                catch (Exception ex)
                {
                    exception = ex;
                }
            });

            if (exception != null)
            {
                loadWindow.Close();
                string errorMessage = _log.GetRecursiveInnerException(exception);
                _log.Log(errorMessage);

                var errorsWindow = new ErrorsWindow("Произошла ошибка.", new List<string>(new string[] { errorMessage }));
                errorsWindow.ShowDialog();
            }
        }
    }
}
