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
    public class ReturnModel : ListViewModel<DocJournalForLoading>
    {
        private Interfaces.IEdoDataBaseAdapter<AbtDbContext> _dataBaseAdapter;
        private WebSystems.Systems.HonestMarkSystem _honestMarkSystem;
        private IEnumerable<DocJournalForLoading> _allDocs;
        private bool _isReturnButtonEnabled;

        public Action<object> OnReturnSelectedCodesProcess;

        public ReturnModel(WebSystems.Systems.HonestMarkSystem honestMarkSystem, Interfaces.IEdoDataBaseAdapter<AbtDbContext> dataBaseAdapter)
        {
            _honestMarkSystem = honestMarkSystem;
            _dataBaseAdapter = dataBaseAdapter;
            InitFromBase();
        }

        public bool IsReturnButtonEnabled => SelectedItem?.Item?.IdDocType == (int)DataContextManagementUnit.DataAccess.DocJournalType.Translocation;

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

        public async Task<Exception> SetDocuments(string code, string comment, DateTime? dateFrom, DateTime? dateTo)
        {
            Exception exception = null;
            await Task.Run(() =>
            {
                try
                {
                    var items = _allDocs;

                    if (dateFrom != null)
                        items = items.Where(i => i.Item?.DocDatetime >= dateFrom);

                    if (dateTo != null)
                        items = items.Where(i => i.Item?.DocDatetime <= dateTo);

                    if (!string.IsNullOrEmpty(code))
                        items = items.Where(i => i.Item?.Code?.Contains(code) ?? false);

                    if (!string.IsNullOrEmpty(comment))
                        items = items.Where(i => i.Item?.Comment?.Contains(comment) ?? false);

                    ItemsList = new System.Collections.ObjectModel.ObservableCollection<DocJournalForLoading>(items);
                    SelectedItem = null;
                    OnPropertyChanged("SelectedItem");
                    OnPropertyChanged("ItemsList");
                }
                catch (Exception ex)
                {
                    string errorMessage = _log.GetRecursiveInnerException(exception);
                    _log.Log(errorMessage);
                    exception = ex;
                }
            });

            return exception;
        }

        private void InitFromBase()
        {
            _allDocs = _dataBaseAdapter?
                .GetJournalMarkedDocumentsByType((int)DataContextManagementUnit.DataAccess.DocJournalType.Translocation)?
                .Cast<DocJournal>()?
                .Select(d => new DocJournalForLoading
                {
                    Item = d,
                    DocEdoReturnPurchasing = _dataBaseAdapter.GetDocEdoReturnPurchasing(d.Id)
                });
        }

        private void LoadStatus(DocEdoReturnPurchasing doc)
        {
            if (_dataBaseAdapter?.GetType() == typeof(Implementations.DiadocEdoToDataBase))
            {
                ((Implementations.DiadocEdoToDataBase)_dataBaseAdapter).LoadStatus(doc);
            }
        }

        public override void Refresh()
        {
            if (ItemsList == null)
                return;

            var docsProcessing = ItemsList.Where(i => i.ReturnStatus == (int)WebSystems.DocEdoStatus.Sent) ?? new List<DocJournalForLoading>();

            if (docsProcessing.Count() == 0)
                return;

            using (var transaction = _dataBaseAdapter.BeginTransaction())
            {
                int i = 1;
                foreach (var docProcessing in docsProcessing)
                {
                    var processingDocument = docProcessing.DocEdoReturnPurchasing as DocEdoReturnPurchasing;
                    try
                    {
                        ((Oracle.ManagedDataAccess.Client.OracleTransaction)transaction.UnderlyingTransaction).Save($"ReturnProcessingDocument_{i}");
                        if (_honestMarkSystem != null)
                        {
                            var docProcessingInfo = _honestMarkSystem.GetEdoDocumentProcessInfo(processingDocument.SellerFileName);

                            if (docProcessingInfo.Code == WebSystems.EdoLiteProcessResultStatus.SUCCESS)
                            {
                                processingDocument.DocStatus = (int)WebSystems.DocEdoStatus.Processed;
                                LoadStatus(processingDocument);
                                _dataBaseAdapter.Commit();
                            }
                            else if (docProcessingInfo.Code == WebSystems.EdoLiteProcessResultStatus.FAILED)
                            {
                                processingDocument.DocStatus = (int)WebSystems.DocEdoStatus.ProcessingError;

                                var failedOperations = docProcessingInfo?.Operations?.Select(o => o.Details)?.Where(o => o.Successful == false);

                                var errors = failedOperations.SelectMany(f => f.Errors);

                                var errorsListStr = new List<string>();
                                foreach (var error in errors)
                                {
                                    if (!string.IsNullOrEmpty(error.Text))
                                        errorsListStr.Add($"Произошла ошибка с кодом:{error.Code} \nОписание:{error.Text}\n");
                                    else if (!string.IsNullOrEmpty(error?.Error?.Detail))
                                        errorsListStr.Add($"Произошла ошибка с кодом:{error.Code} \nДетали:{error?.Error?.Detail}\n");
                                    else
                                        errorsListStr.Add($"Произошла ошибка с кодом:{error.Code}\n");
                                }
                                processingDocument.ErrorMessage = string.Join("\n\n", errorsListStr);
                                LoadStatus(processingDocument);
                                _dataBaseAdapter.Commit();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        ((Oracle.ManagedDataAccess.Client.OracleTransaction)transaction.UnderlyingTransaction).Rollback($"ReturnProcessingDocument_{i}");
                        _dataBaseAdapter.ReloadEntry(processingDocument);
                        string errorMessage = _log.GetRecursiveInnerException(ex);
                        _log.Log(errorMessage);

                        //var errorsWindow = new ErrorsWindow("Произошла ошибка обновления статусов.", new List<string>(new string[] { errorMessage }));
                        //errorsWindow.ShowDialog();
                    }

                    i++;
                }

                if (docsProcessing.Any(p => p.ReturnStatus != (int)WebSystems.DocEdoStatus.Sent))
                    transaction.Commit();
            }
        }
    }
}
