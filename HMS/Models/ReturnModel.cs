using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using Reporter.Entities;
using UtilitesLibrary.ModelBase;
using DataContextManagementUnit.DataAccess.Contexts.Abt;

namespace HonestMarkSystem.Models
{
    public class ReturnModel : ListViewModel<DocJournal>
    {
        private IEnumerable<DocJournal> _allDocs;
        private bool _isReturnButtonEnabled;

        public Action<object> OnReturnSelectedCodesProcess;

        public ReturnModel(IEnumerable<DocJournal> allDocs)
        {
            _allDocs = allDocs;
        }

        public bool IsReturnButtonEnabled => SelectedItem?.IdDocType == (int)DataContextManagementUnit.DataAccess.DocJournalType.Translocation;

        public async void ReturnCodes()
        {
            if (SelectedItem == null)
            {
                System.Windows.MessageBox.Show(
                    "Документ не выбран.", "Ошибка", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                return;
            }

            var loadWindow = new LoadWindow("Начало процесса");

            //if (owner != null)
            //    loadWindow.Owner = owner;

            var loadContext = loadWindow.GetLoadContext();
            loadWindow.Show();

            var loadActionContext = new Implementations.LoadActionData
            {
                InputData = new List<object> { loadContext },
                ErrorMessage = null,
                TitleErrorText = null,
                Errors = new List<string>()
            };

            var task = new Task(OnReturnSelectedCodesProcess, loadActionContext);
            task.Start();
            await Task.WhenAll(task);

            if (!string.IsNullOrEmpty(loadActionContext.ErrorMessage))
            {
                loadWindow.Close();
                _log.Log(loadActionContext.ErrorMessage);
                var errorsWindow = new ErrorsWindow(loadActionContext.TitleErrorText, new List<string>(new string[] { loadActionContext.ErrorMessage }));
                errorsWindow.ShowDialog();
            }
            else if (loadActionContext.Errors.Count > 0)
            {
                loadWindow.Close();
                var errorsWindow = new ErrorsWindow("В списке кодов есть непропиканные, либо не оприходованные коды", loadActionContext.Errors);
                errorsWindow.ShowDialog();
            }
        }

        public async void SetDocuments(string code, string comment, DateTime? dateFrom, DateTime? dateTo, System.Windows.Window owner = null)
        {
            var loadWindow = new LoadWindow("Идёт поиск...");

            if (owner != null)
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
