using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebSystems;
using WebSystems.EdoSystems;
using UtilitesLibrary.ModelBase;
using WebSystems.Models;
using DataContextManagementUnit.DataAccess.Contexts.Abt;
using System.Xml;
using System.IO;
using System.IO.Compression;
using HonestMarkSystem.Implementations;

namespace HonestMarkSystem.Models
{
    public class MainViewModel : ListViewModel<DocEdoPurchasing>, Interfaces.IEdoDocumentsView
    {
        private const string edoFilesPath = "Files";
        private const string _applicationName = "Вирэй Приходная";
        private string currentVersion => System.Reflection.Assembly.GetExecutingAssembly()?.GetName()?.Version?.ToString();

        
        private Interfaces.IEdoDataBaseAdapter<AbtDbContext> _dataBaseAdapter;

        public string EdoProgramVersion => $"{_applicationName} {currentVersion}";

        public List<DocEdoPurchasingDetail> Details => SelectedItem?.Details;
        public DocEdoPurchasingDetail SelectedDetail { get; set; }

        public List<ConsignorOrganization> MyOrganizations { get; set; }
        public ConsignorOrganization SelectedMyOrganization { get; set; }

        public DateTime DateTo { get; set; } = DateTime.Now.AddDays(1);
        public DateTime DateFrom { get; set; } = DateTime.Now.AddMonths(-6);

        public bool IsRevokedDocument => SelectedItem?.DocStatus == (int?)DocEdoStatus.RevokeRequired || SelectedItem?.DocStatus == (int?)DocEdoStatus.Processed
            || SelectedItem?.DocStatus == (int?)DocEdoStatus.ProcessingError;

        public override RelayCommand RefreshCommand => new RelayCommand((o) => { Refresh(); });
        public override RelayCommand EditCommand => new RelayCommand((o) => { Save(); });
        public override RelayCommand DeleteCommand => new RelayCommand((o) => { UnbindDocument(); });
        public RelayCommand ChangePurchasingDocumentCommand => new RelayCommand((o) => { ChangePurchasingDocument(); });
        public RelayCommand SignAndSendCommand => new RelayCommand((o) => { SignAndSend(); });
        public RelayCommand ExportToTraderCommand => new RelayCommand((o) => { ExportToTrader(); });
        public RelayCommand ReturnMarkedCodesCommand => new RelayCommand((o) => { ReturnMarkedCodes(); });
        public RelayCommand WithdrawalCodesCommand => new RelayCommand((o) => { WithdrawalCodes(); });
        public RelayCommand RejectDocumentCommand => new RelayCommand((o) => { RejectDocument(); });
        public RelayCommand RevokeDocumentCommand => new RelayCommand((o) => { RevokeDocument(); });
        public RelayCommand SaveDocumentFileCommand => new RelayCommand((o) => { SaveDocumentFile(); });

        public MainViewModel(Interfaces.IEdoDataBaseAdapter<AbtDbContext> dataBaseAdapter, List<ConsignorOrganization> myOrganizations)
        {
            _dataBaseAdapter = dataBaseAdapter;
            MyOrganizations = myOrganizations;

            SelectedMyOrganization = null;
            SelectedItem = null;

            ItemsList = new System.Collections.ObjectModel.ObservableCollection<DocEdoPurchasing>();
        }

        public System.Windows.Window Owner { get; set; }

        private void Refresh()
        {
            _dataBaseAdapter.Dispose();
            _dataBaseAdapter.InitializeContext();

            foreach (var myOrganization in MyOrganizations)
            {
                if (!(myOrganization.EdoSystem?.Authorization() ?? false))
                {
                    myOrganization.EdoSystem = null;
                    continue;
                }

                if (!(myOrganization.HonestMarkSystem?.Authorization() ?? false))
                {
                    myOrganization.HonestMarkSystem = null;
                }

                object[] parameters = null;

                List<IEdoSystemDocument<string>> documents = new List<IEdoSystemDocument<string>>();
                try
                {
                    documents = GetNewDocuments(myOrganization, out parameters);
                }
                catch (System.Net.WebException webEx)
                {
                    string errorMessage = _log.GetRecursiveInnerException(webEx);
                    _log.Log(errorMessage);

                    var errorsWindow = new ErrorsWindow("Произошла ошибка получения документов на удалённом сервере.", new List<string>(new string[] { errorMessage }));
                    errorsWindow.ShowDialog();
                }
                catch (Exception ex)
                {
                    string errorMessage = _log.GetRecursiveInnerException(ex);
                    _log.Log(errorMessage);

                    var errorsWindow = new ErrorsWindow("Произошла ошибка получения документов.", new List<string>(new string[] { errorMessage }));
                    errorsWindow.ShowDialog();
                }

                if (documents.Count > 0)
                {
                    using (var newDocsTransaction = _dataBaseAdapter.BeginTransaction())
                    {
                        try
                        {
                            foreach (var doc in documents)
                            {
                                if (!_dataBaseAdapter.DocumentCanBeAddedByUser(myOrganization, doc))
                                    continue;

                                byte[] docContentBytes;

                                if (!SaveNewDocument(myOrganization, doc, out docContentBytes))
                                    continue;

                                var docFromDb = (DocEdoPurchasing)_dataBaseAdapter.GetDocumentFromDb(doc);

                                if (docFromDb == null)
                                {
                                    myOrganization.EdoSystem.SendReceivingConfirmationEventHandler?.Invoke(this, new WebSystems.EventArgs.SendReceivingConfirmationEventArgs { Document = doc });
                                    _dataBaseAdapter.AddDocumentToDataBase(myOrganization, doc, docContentBytes, DocumentInOutType.Inbox);
                                }
                            }
                            _dataBaseAdapter.Commit(newDocsTransaction);
                            SaveParameters(myOrganization, parameters);
                        }
                        catch (Exception ex)
                        {
                            newDocsTransaction.Rollback();
                            string errorMessage = _log.GetRecursiveInnerException(ex);
                            _log.Log(errorMessage);

                            var errorsWindow = new ErrorsWindow("Произошла ошибка.", new List<string>(new string[] { errorMessage }));
                            errorsWindow.ShowDialog();
                        }
                    }
                }

                var docs = _dataBaseAdapter.GetAllDocuments(DateFrom, DateTo).Cast<DocEdoPurchasing>()
                    .Where(d => d.ReceiverInn == myOrganization.OrgInn && myOrganization.ShipperOrgInns.Exists(s => s == d.SenderInn));

                try
                {
                    var errorsList = new List<string>();
                    var processingDocuments = docs.Where(d => d.DocStatus == (int)DocEdoStatus.Sent).ToList() ?? new List<DocEdoPurchasing>();

                    if (myOrganization.HonestMarkSystem != null)
                    {
                        var honestMarkSystem = myOrganization.HonestMarkSystem;

                        using (var transaction = _dataBaseAdapter.BeginTransaction())
                        {
                            int i = 1;
                            foreach (var processingDocument in processingDocuments)
                            {
                                try
                                {
                                    ((Oracle.ManagedDataAccess.Client.OracleTransaction)transaction.UnderlyingTransaction).Save($"ProcessingDocumentPoint_{i}");
                                    if (honestMarkSystem != null)
                                    {
                                        var docProcessingInfo = honestMarkSystem.GetEdoDocumentProcessInfo(processingDocument.FileName);

                                        if (docProcessingInfo.Code == EdoLiteProcessResultStatus.SUCCESS)
                                        {
                                            processingDocument.DocStatus = (int)DocEdoStatus.Processed;
                                            LoadStatus(processingDocument);

                                            if (processingDocument.IdDocJournal != null)
                                                _dataBaseAdapter.UpdateMarkedCodeIncomingStatuses(processingDocument.IdDocJournal.Value, MarkedCodeComingStatus.Accepted);

                                            _dataBaseAdapter.Commit();
                                        }
                                        else if (docProcessingInfo.Code == EdoLiteProcessResultStatus.FAILED)
                                        {
                                            processingDocument.DocStatus = (int)DocEdoStatus.ProcessingError;

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
                                    ((Oracle.ManagedDataAccess.Client.OracleTransaction)transaction.UnderlyingTransaction).Rollback($"ProcessingDocumentPoint_{i}");
                                    _dataBaseAdapter.ReloadEntry(processingDocument);
                                    string errorMessage = _log.GetRecursiveInnerException(ex);
                                    _log.Log(errorMessage);

                                    //var errorsWindow = new ErrorsWindow("Произошла ошибка обновления статусов.", new List<string>(new string[] { errorMessage }));
                                    //errorsWindow.ShowDialog();
                                }

                                i++;
                            }

                            if (processingDocuments.Exists(p => p.DocStatus != (int)DocEdoStatus.Sent))
                                transaction.Commit();
                        }
                    }

                    var newDocuments = docs.Where(d => d.DocStatus == (int)DocEdoStatus.New || d.DocStatus == (int)DocEdoStatus.RevokeRequested).ToList() ?? new List<DocEdoPurchasing>();

                    using (var transaction = _dataBaseAdapter.BeginTransaction())
                    {
                        foreach (var newDocument in newDocuments)
                        {
                            try
                            {
                                if (myOrganization.EdoSystem.GetType() == typeof(DiadocEdoSystem))
                                    newDocument.DocStatus = (int)myOrganization.EdoSystem.GetCurrentStatus(newDocument.DocStatus, newDocument.IdDocEdo, newDocument.ParentEntityId);

                                _dataBaseAdapter.Commit();
                            }
                            catch (Exception ex)
                            {
                                _dataBaseAdapter.ReloadEntry(newDocument);
                                string errorMessage = _log.GetRecursiveInnerException(ex);
                                _log.Log(errorMessage);

                                errorsList.Add(errorMessage);
                            }
                        }

                        transaction.Commit();
                    }

                    if (errorsList.Count > 0)
                    {
                        var errorsWindow = new ErrorsWindow("Произошли ошибки.", errorsList);
                        errorsWindow.ShowDialog();
                    }

                }
                catch (Exception ex)
                {
                    string errorMessage = _log.GetRecursiveInnerException(ex);
                    _log.Log(errorMessage);

                    var errorsWindow = new ErrorsWindow("Произошла ошибка обновления статусов документов.", new List<string>(new string[] { errorMessage }));
                    errorsWindow.ShowDialog();
                }
            }

            ItemsList = new System.Collections.ObjectModel.ObservableCollection<DocEdoPurchasing>();
            SelectedItem = null;
            SelectedMyOrganization = null;
            UpdateProperties();
        }

        private void Save()
        {
            try
            {
                _dataBaseAdapter.Commit();
            }
            catch(Exception ex)
            {
                //_dataBaseAdapter.Rollback();
                var errorMessage = $"Exception: {_log.GetRecursiveInnerException(ex)}";
                var errorsWindow = new ErrorsWindow("Произошла ошибка сохранения базы данных.", new List<string>(new string[] { errorMessage }));
                _log.Log(errorMessage);
                errorsWindow.ShowDialog();
            }
        }

        private void UnbindDocument()
        {
            if (SelectedItem == null)
            {
                System.Windows.MessageBox.Show(
                    "Не выбран документ для отвязывания.", "Ошибка", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                return;
            }

            if (SelectedItem.IdDocJournal == null)
            {
                System.Windows.MessageBox.Show(
                    "Данный документ не сопоставлен с документом из трейдера.", "Ошибка", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                return;
            }

            if (SelectedItem.DocStatus == (int)DocEdoStatus.Sent)
            {
                System.Windows.MessageBox.Show(
                    "Данный документ был подписан и отправлен, его нельзя отвязать.", "Ошибка", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                return;
            }

            if (SelectedItem.DocStatus == (int)DocEdoStatus.Processed)
            {
                System.Windows.MessageBox.Show(
                    "Данный документ уже был успешно обработан, его нельзя отвязать.", "Ошибка", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                return;
            }

            using (var transaction = _dataBaseAdapter.BeginTransaction())
            {
                try
                {
                    SelectedItem.IdDocJournal = null;
                    OnPropertyChanged("SelectedItem");
                    _dataBaseAdapter.Commit(transaction);

                    LoadWindow loadWindow = new LoadWindow();
                    loadWindow.GetLoadContext().SetSuccessFullLoad("Документ отвязан успешно.");
                    loadWindow.ShowDialog();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    _dataBaseAdapter.ReloadEntry(SelectedItem);
                    var errorMessage = $"Exception: {_log.GetRecursiveInnerException(ex)}";
                    var errorsWindow = new ErrorsWindow("Произошла ошибка отвязки документа.", new List<string>(new string[] { errorMessage }));
                    _log.Log(errorMessage);
                    errorsWindow.ShowDialog();
                }
            }
        }

        private async void ChangePurchasingDocument()
        {
            if(SelectedItem == null)
            {
                System.Windows.MessageBox.Show(
                    "Не выбран документ для сопоставления.", "Ошибка", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                return;
            }

            if (SelectedMyOrganization == null)
            {
                System.Windows.MessageBox.Show(
                    "Не выбрана своя организация для сопоставления.", "Ошибка", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                return;
            }

            var honestMarkSystem = SelectedMyOrganization.HonestMarkSystem;
            var docs = _dataBaseAdapter.GetJournalDocuments(SelectedItem);

            var docPurchasingWindow = new PurchasingDocumentsWindow();
            var docPurchasingModel = new PurchasingDocumentsModel(docs.Cast<DocJournal>());

            if (SelectedItem.IdDocJournal != null)
                docPurchasingModel.SetChangedDocument(SelectedItem.IdDocJournal.Value);

            docPurchasingWindow.DataContext = docPurchasingModel;
            if(docPurchasingWindow.ShowDialog() == true)
            {
                string errorMessage = null;

                LoadWindow loadWindow = new LoadWindow("Подождите, идёт сопоставление");

                if (this.Owner != null)
                    loadWindow.Owner = this.Owner;

                var loadContext = loadWindow.GetLoadContext();

                loadWindow.Show();
                await Task.Run(() =>
                {
                    using (var transaction = _dataBaseAdapter.BeginTransaction())
                    {
                        decimal? oldIdDoc = SelectedItem?.IdDocJournal;
                        try
                        {
                            SelectedItem.IdDocJournal = docPurchasingModel.SelectedItem.Id;

                            if (docPurchasingModel.SelectedItem?.IdDocType == (int?)DataContextManagementUnit.DataAccess.DocJournalType.Receipt)
                            {
                                loadContext.SetLoadingText("Привязывание кодов");

                                foreach(var detail in Details ?? new List<DocEdoPurchasingDetail>())
                                {
                                    if (string.IsNullOrEmpty(detail.BarCode))
                                        continue;

                                    var refGoodsObj = _dataBaseAdapter?.GetRefGoodsByBarCode(detail.BarCode);

                                    if (refGoodsObj == null || refGoodsObj.Count() == 0)
                                        continue;

                                    var refGoods = refGoodsObj.Cast<RefGood>();

                                    var selectedDocJournal = docPurchasingModel.SelectedItem;
                                    var refGood = refGoods.FirstOrDefault(r => selectedDocJournal.Details.Exists(d => d.IdGood == r.Id));

                                    if (refGood != null)
                                        detail.IdGood = refGood.Id;
                                }

                                if (honestMarkSystem != null)
                                {
                                    loadContext.SetLoadingText("Сохранение кодов");
                                    var sellerXmlDocument = new XmlDocument();
                                    sellerXmlDocument.Load($"{edoFilesPath}//{SelectedItem.IdDocEdo}//{SelectedItem.FileName}.xml");
                                    var docSellerContent = Encoding.GetEncoding(1251).GetBytes(sellerXmlDocument.OuterXml);
                                    SaveMarkedCodesToDataBase(docSellerContent, SelectedItem.IdDocJournal, oldIdDoc, Details?.Where(d => d.IdGood != null)?.Select(d => d.IdGood.Value)?.ToList());
                                }
                            }
                            else if(docPurchasingModel.SelectedItem?.IdDocType == (int?)DataContextManagementUnit.DataAccess.DocJournalType.Translocation)
                            {
                                loadContext.SetLoadingText("Привязывание кодов");

                                foreach (var detail in Details)
                                {
                                    var idGoods = _dataBaseAdapter.GetRefGoodsByBarCode(detail.BarCode)?
                                    .Where(r => (r as RefGood)?.Id != null)?
                                    .Select(r => (r as RefGood).Id) ?? new List<decimal>();

                                    var docLine = docPurchasingModel.SelectedItem.Details?
                                    .FirstOrDefault(g => idGoods.Any(i => i == g.IdGood));

                                    if (docLine != null)
                                        detail.IdGood = docLine.IdGood;
                                }
                            }

                            _dataBaseAdapter.Commit(transaction);
                            OnPropertyChanged("SelectedItem");
                            loadContext.SetSuccessFullLoad("Документ был успешно сопоставлен.");
                        }
                        catch (Exception ex)
                        {
                            SelectedItem.IdDocJournal = oldIdDoc;
                            transaction.Rollback();
                            _dataBaseAdapter.ReloadEntry(SelectedItem);
                            errorMessage = _log.GetRecursiveInnerException(ex);
                            _log.Log(errorMessage);
                        }
                    }
                });

                if (errorMessage != null)
                {
                    loadWindow.Close();
                    var errorsWindow = new ErrorsWindow("Произошла ошибка сопоставления трейдер-документа.", new List<string>(new string[] { errorMessage }));
                    errorsWindow.ShowDialog();
                }
            }
        }

        private void LoadStatus(DocEdoPurchasing doc)
        {
            if(_dataBaseAdapter?.GetType() == typeof(DiadocEdoToDataBase))
            {
                ((DiadocEdoToDataBase)_dataBaseAdapter).LoadStatus(doc);
            }
        }

        private async void SignAndSend()
        {
            if (SelectedMyOrganization == null)
            {
                System.Windows.MessageBox.Show(
                    "Не выбрана своя организация для подписи документов.", "Ошибка", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                return;
            }

            if (SelectedItem == null)
            {
                System.Windows.MessageBox.Show(
                    "Не выбран документ для подписания.", "Ошибка", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                return;
            }

            if(SelectedItem.DocStatus == (int)DocEdoStatus.Sent)
            {
                System.Windows.MessageBox.Show(
                    "Данный документ ранее уже был подписан и отправлен.", "Ошибка", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                return;
            }

            if (SelectedItem.DocStatus == (int)DocEdoStatus.Processed)
            {
                System.Windows.MessageBox.Show(
                    "Данный документ уже был подписан, отправлен и успешно обработан.", "Ошибка", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                return;
            }

            if (SelectedItem.DocStatus == (int)DocEdoStatus.Rejected)
            {
                System.Windows.MessageBox.Show(
                    "Данный документ был отклонён.", "Ошибка", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                return;
            }

            if (SelectedItem.DocStatus == (int)DocEdoStatus.NoSignatureRequired)
            {
                System.Windows.MessageBox.Show(
                    "Данный документ не предназначен для подписания.", "Ошибка", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                return;
            }

            if(SelectedItem.DocStatus == (int)DocEdoStatus.RevokeRequired || SelectedItem.DocStatus == (int)DocEdoStatus.RevokeRequested)
            {
                System.Windows.MessageBox.Show(
                    "Данный документ находится в статусе, ожидающем аннулирование.", "Ошибка", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                return;
            }

            if (SelectedItem.DocStatus == (int)DocEdoStatus.Revoked)
            {
                System.Windows.MessageBox.Show(
                    "Данный документ аннулирован.", "Ошибка", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                return;
            }

            if (SelectedMyOrganization.EdoSystem == null)
            {
                System.Windows.MessageBox.Show(
                    "Регистрация в веб сервисе не была успешной.", "Ошибка", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                return;
            }

            if (SelectedItem.IdDocJournal == null)
            {
                System.Windows.MessageBox.Show(
                    "Данный документ не сопоставлен с документом из трейдера.", "Ошибка", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                return;
            }

            DocJournal docJournal = null;
            try
            {
                docJournal = _dataBaseAdapter.GetDocJournal(SelectedItem.IdDocJournal.Value) as DocJournal;
            }
            catch (Exception ex)
            {
                string errorMessage = _log.GetRecursiveInnerException(ex);
                _log.Log(errorMessage);

                var errorsWindow = new ErrorsWindow("Произошла ошибка поиска трейдер документа в базе.", new List<string>(new string[] { errorMessage }));
                errorsWindow.ShowDialog();
                return;
            }

            if (docJournal == null)
            {
                System.Windows.MessageBox.Show(
                    "Не найден трейдер документ.", "Ошибка", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                return;
            }

            if (docJournal.IdDocType == (int)DataContextManagementUnit.DataAccess.DocJournalType.Translocation && docJournal.ActStatus != (int)DataContextManagementUnit.DataAccess.DocJournalActStatus.Confirmed)
            {
                System.Windows.MessageBox.Show(
                    "Для трейдер документа перемещения статус документа некорректный для подписания.", "Ошибка", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                return;
            }

            var edoSystem = SelectedMyOrganization.EdoSystem;
            var honestMarkSystem = SelectedMyOrganization.HonestMarkSystem;
            var cryptoUtil = SelectedMyOrganization.CryptoUtil;

            if (edoSystem.HasZipContent && !File.Exists($"{edoFilesPath}//{SelectedItem.IdDocEdo}//{SelectedItem.FileName}.zip"))
            {
                System.Windows.MessageBox.Show(
                    "Не найден zip файл документооборота.", "Ошибка", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                return;
            }

            if (Details.Where(d => d.IdGood != null).GroupBy(d => d.IdGood).Any(g => g.Count() > 1))
            {
                System.Windows.MessageBox.Show(
                    "В списке товаров есть одинаковые ID товаров.", "", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                return;
            }

            if (!File.Exists($"{edoFilesPath}//{SelectedItem.IdDocEdo}//{SelectedItem.FileName}.xml"))
            {
                if (edoSystem.HasZipContent)
                {
                    try
                    {
                        using (ZipArchive zipArchive = ZipFile.Open($"{edoFilesPath}//{SelectedItem.IdDocEdo}//{SelectedItem.FileName}.zip", ZipArchiveMode.Read))
                        {
                            var entry = zipArchive.Entries.FirstOrDefault(x => x.Name == $"{SelectedItem.FileName}.xml");
                            entry?.ExtractToFile($"{edoFilesPath}//{SelectedItem.IdDocEdo}//{SelectedItem.FileName}.xml");
                        }
                    }
                    catch (Exception ex)
                    {
                        string errorMessage = _log.GetRecursiveInnerException(ex);
                        _log.Log(errorMessage);

                        var errorsWindow = new ErrorsWindow("Произошла ошибка извлечения файла xml из архива.", new List<string>(new string[] { errorMessage }));
                        errorsWindow.ShowDialog();
                        return;
                    }
                }
                else
                {
                    System.Windows.MessageBox.Show("Не найден xml файл документа.", "Ошибка", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                    return;
                }
            }

            var signWindow = new BuyerSignWindow(cryptoUtil, $"{edoFilesPath}//{SelectedItem.IdDocEdo}//{SelectedItem.FileName}.xml");
            signWindow.SetDefaultParameters(edoSystem.GetCertSubject(), SelectedItem);
            signWindow.Report.EdoProgramVersion = this.EdoProgramVersion;

            string signedFilePath;

            if (edoSystem.HasZipContent)
            {
                try
                {
                    using (ZipArchive zipArchive = ZipFile.Open($"{edoFilesPath}//{SelectedItem.IdDocEdo}//{SelectedItem.FileName}.zip", ZipArchiveMode.Read))
                    {
                        var signedEntry = zipArchive.Entries.FirstOrDefault(x => x.Name.StartsWith(SelectedItem.FileName) && x.Name != $"{SelectedItem.FileName}.xml");

                        if (signedEntry == null)
                            throw new Exception("Не найден файл подписи продавца.");

                        if (!File.Exists($"{edoFilesPath}//{SelectedItem.IdDocEdo}//{signedEntry.Name}"))
                            signedEntry?.ExtractToFile($"{edoFilesPath}//{SelectedItem.IdDocEdo}//{signedEntry.Name}");

                        signedFilePath = $"{edoFilesPath}//{SelectedItem.IdDocEdo}//{signedEntry.Name}";
                    }
                }
                catch (Exception ex)
                {
                    string errMessage = _log.GetRecursiveInnerException(ex);
                    _log.Log(errMessage);

                    var errorsWindow = new ErrorsWindow("Произошла ошибка извлечения файла подписи продавца из архива.", new List<string>(new string[] { errMessage }));
                    errorsWindow.ShowDialog();
                    return;
                }
            }
            else
            {
                signedFilePath = $"{edoFilesPath}//{SelectedItem.IdDocEdo}//{SelectedItem.FileName}.xml.sig";

                if(!File.Exists(signedFilePath))
                {
                    System.Windows.MessageBox.Show("Не найден файл подписи продавца.", "Ошибка", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                    return;
                }
            }

            signWindow.SellerSignature = File.ReadAllBytes(signedFilePath);

            if (signWindow.ShowDialog() == true)
            {
                signWindow.OnAllPropertyChanged();

                LoadWindow loadWindow = new LoadWindow("Подождите, идёт загрузка данных");

                if (this.Owner != null)
                    loadWindow.Owner = this.Owner;

                var reportForSend = signWindow.Report;

                var sellerXmlDocument = new XmlDocument();
                sellerXmlDocument.Load($"{edoFilesPath}//{SelectedItem.IdDocEdo}//{SelectedItem.FileName}.xml");
                var docSellerContent = Encoding.GetEncoding(1251).GetBytes(sellerXmlDocument.OuterXml);

                var loadContext = loadWindow.GetLoadContext();

                string errorMessage = null;
                string titleErrorText = null;

                var errorsPostingCodes = new List<string>();

                loadWindow.Show();

                await Task.Run(() => 
                {
                    using (var transaction = _dataBaseAdapter.BeginTransaction())
                    {
                        try
                        {
                            var markedCodesArray = _dataBaseAdapter.GetMarkedCodesByDocumentId(SelectedItem.IdDocJournal) ?? new List<string>();

                            if (markedCodesArray.Count() > 0)
                            {
                                if (honestMarkSystem != null && !MarkedCodesOwnerCheck(markedCodesArray, SelectedItem.SenderInn))
                                    throw new Exception("В списке кодов маркировки есть не принадлежащие отправителю.");

                                if (_dataBaseAdapter.IsExistsNotReceivedCodes(SelectedItem.IdDocJournal.Value, Convert.ToInt32(docJournal.IdDocType)))
                                {
                                    errorsPostingCodes = _dataBaseAdapter.GetErrorsWithMarkedCodes(SelectedItem.IdDocJournal.Value, Convert.ToInt32(docJournal.IdDocType));
                                    return;
                                }
                            }
                            var xml = reportForSend.GetXmlContent();
                            var fileBytes = Encoding.GetEncoding(1251).GetBytes(xml);
                            var signature = cryptoUtil.Sign(fileBytes, true);

                            File.WriteAllBytes($"{edoFilesPath}//{SelectedItem.IdDocEdo}//{reportForSend.FileName}.xml", fileBytes);
                            File.WriteAllBytes($"{edoFilesPath}//{SelectedItem.IdDocEdo}//{reportForSend.FileName}.xml.sig", signature);

                            var directory = new DirectoryInfo(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location));

                            loadContext.SetLoadingText("Подписание и отправка");
                            if (edoSystem.GetType() == typeof(EdoLiteSystem))
                            {
                                string localPath = directory.Name;
                                while (directory.Parent != null)
                                {
                                    directory = directory.Parent;

                                    if (directory.Parent == null)
                                        localPath = $"{directory.Name.Replace(":\\", ":")}/{localPath}";
                                    else
                                        localPath = $"{directory.Name}/{localPath}";
                                }

                                string content = $"{localPath}/{edoFilesPath}/{SelectedItem.IdDocEdo}/{reportForSend.FileName}.xml";
                                edoSystem.SendDocument(SelectedItem.IdDocEdo, fileBytes, signature, SelectedMyOrganization.EmchdId, content);
                            }
                            else if(edoSystem.GetType() == typeof(DiadocEdoSystem))
                            {
                                string typeNameId = string.Empty, function = string.Empty;

                                if (SelectedItem.IdDocType == (int)Diadoc.Api.Proto.DocumentType.UniversalTransferDocument ||
                                SelectedItem.IdDocType == (int)Diadoc.Api.Proto.DocumentType.UniversalTransferDocumentRevision)
                                {
                                    typeNameId = "UniversalTransferDocumentBuyerTitle";
                                    function = "СЧФДОП";
                                }
                                else if (SelectedItem.IdDocType == (int)Diadoc.Api.Proto.DocumentType.XmlTorg12)
                                {
                                    typeNameId = "XmlTorg12BuyerTitle";
                                    function = "ДОП";
                                }
                                else if (SelectedItem.IdDocType == (int)Diadoc.Api.Proto.DocumentType.XmlAcceptanceCertificate)
                                {
                                    typeNameId = "XmlAcceptanceCertificateBuyerTitle";
                                    function = "ДОП";
                                }
                                else
                                {
                                    System.Windows.MessageBox.Show("Исходный документ продавца не предусматривает подписи и отправки титула покупателя.", "Ошибка", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                                    return;
                                }

                                edoSystem.SendDocument(SelectedItem.IdDocEdo, fileBytes, signature, SelectedMyOrganization.EmchdId, SelectedItem.ParentEntityId, SelectedItem.IdDocType);
                            }

                            SelectedItem.SignatureFileName = reportForSend.FileName;

                            if (reportForSend.FileName.StartsWith("ON_NSCHFDOPPOKMARK"))
                                SelectedItem.DocStatus = (int)DocEdoStatus.Sent;
                            else
                                SelectedItem.DocStatus = (int)DocEdoStatus.Processed;

                            LoadStatus(SelectedItem);
                            UpdateProperties();

                            _dataBaseAdapter.Commit(transaction);
                            loadContext.SetSuccessFullLoad("Данные были успешно загружены.");
                        }
                        catch (System.Net.WebException webEx)
                        {
                            transaction.Rollback();
                            _dataBaseAdapter.ReloadEntry(SelectedItem);
                            errorMessage = _log.GetRecursiveInnerException(webEx);
                            titleErrorText = "Произошла ошибка отправки на удалённом сервере.";
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            _dataBaseAdapter.ReloadEntry(SelectedItem);
                            errorMessage = _log.GetRecursiveInnerException(ex);
                            titleErrorText = "Произошла ошибка отправки.";
                        }
                    }
                });

                if (!string.IsNullOrEmpty(errorMessage))
                {
                    loadWindow.Close();
                    _log.Log(errorMessage);
                    var errorsWindow = new ErrorsWindow(titleErrorText, new List<string>(new string[] { errorMessage }));
                    errorsWindow.ShowDialog();
                }
                else if(errorsPostingCodes.Count > 0)
                {
                    loadWindow.Close();
                    _log.Log(errorMessage);
                    var errorsWindow = new ErrorsWindow("В списке кодов есть непропиканные, либо оприходованные коды", errorsPostingCodes);
                    errorsWindow.ShowDialog();
                }
            }
        }

        private async void ExportToTrader()
        {
            if (SelectedMyOrganization == null)
            {
                System.Windows.MessageBox.Show(
                    "Не выбрана своя организация для экспорта документов.", "Ошибка", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                return;
            }

            if (SelectedItem == null)
            {
                System.Windows.MessageBox.Show(
                    "Не выбран документ для экспорта.", "Ошибка", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                return;
            }

            if (Details.Exists(d => d?.IdGood == null))
            {
                System.Windows.MessageBox.Show(
                    "В списке товаров есть несопоставленные с ID товары.", "", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                return;
            }

            if (SelectedItem.IdDocJournal != null)
            {
                System.Windows.MessageBox.Show(
                    "Данный документ уже сопоставлен с документом в трейдере.", "Ошибка", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                return;
            }

            var honestMarkSystem = SelectedMyOrganization.HonestMarkSystem;
            if (honestMarkSystem == null)
            {
                if(System.Windows.MessageBox.Show(
                    "Авторизация в Честном знаке не была успешной. \nНевозможно экспортировать коды маркировки.\nВы точно хотите экспортировать документ?", "Ошибка авторизации", System.Windows.MessageBoxButton.YesNo, System.Windows.MessageBoxImage.Question) != System.Windows.MessageBoxResult.Yes)
                    return;
            }

            var edoSystem = SelectedMyOrganization.EdoSystem;
            string errorMessage = null;
            if (!File.Exists($"{edoFilesPath}//{SelectedItem.IdDocEdo}//{SelectedItem.FileName}.xml"))
            {
                if (edoSystem?.HasZipContent ?? false)
                {
                    try
                    {
                        using (ZipArchive zipArchive = ZipFile.Open($"{edoFilesPath}//{SelectedItem.IdDocEdo}//{SelectedItem.FileName}.zip", ZipArchiveMode.Read))
                        {
                            var entry = zipArchive.Entries.FirstOrDefault(x => x.Name == $"{SelectedItem.FileName}.xml");
                            entry?.ExtractToFile($"{edoFilesPath}//{SelectedItem.IdDocEdo}//{SelectedItem.FileName}.xml");
                        }
                    }
                    catch (Exception ex)
                    {
                        errorMessage = _log.GetRecursiveInnerException(ex);
                        _log.Log(errorMessage);

                        var errorsWindow = new ErrorsWindow("Произошла ошибка извлечения файла xml из архива.", new List<string>(new string[] { errorMessage }));
                        errorsWindow.ShowDialog();
                        return;
                    }
                }
                else
                {
                    System.Windows.MessageBox.Show("Не найден xml файл документа.", "Ошибка", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                    return;
                }
            }

            LoadWindow loadWindow = new LoadWindow();
            var loadContext = loadWindow.GetLoadContext();
            loadWindow.Show();

            await Task.Run(() =>
            {
                using (var transaction = _dataBaseAdapter.BeginTransaction())
                {
                    try
                    {
                        var sellerXmlDocument = new XmlDocument();
                        sellerXmlDocument.Load($"{edoFilesPath}//{SelectedItem.IdDocEdo}//{SelectedItem.FileName}.xml");
                        var docSellerContent = Encoding.GetEncoding(1251).GetBytes(sellerXmlDocument.OuterXml);

                        loadContext.SetLoadingText("Сохранение документа");
                        var docJournalId = _dataBaseAdapter.ExportDocument(SelectedItem);

                        if (honestMarkSystem != null)
                        {
                            loadContext.SetLoadingText("Сохранение кодов маркировки");
                            SaveMarkedCodesToDataBase(docSellerContent, docJournalId, null, Details?.Select(d => d.IdGood.Value)?.ToList());
                        }

                        _dataBaseAdapter.Commit(transaction);
                        loadContext.SetSuccessFullLoad("Экспорт выполнен успешно.");
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        _dataBaseAdapter.ReloadEntry(SelectedItem);
                        errorMessage = _log.GetRecursiveInnerException(ex);
                    }
                }
            });

            if (!string.IsNullOrEmpty(errorMessage))
            {
                loadWindow.Close();
                _log.Log(errorMessage);
                var errorsWindow = new ErrorsWindow("Произошла ошибка экспорта.", new List<string>(new string[] { errorMessage }));
                errorsWindow.ShowDialog();
            }
        }

        private void ReturnMarkedCodes()
        {
            var returnModel = new ReturnModel(MyOrganizations, _dataBaseAdapter);
            var returnWindow = new ReturnWindow();
            returnWindow.DataContext = returnModel;

            returnModel.OnReturnSelectedCodesProcess = (object s) =>
            {
                var loadActionContext = s as LoadActionData;
                var loadContext = loadActionContext.InputData.FirstOrDefault() as LoadModel;

                try
                {
                    var docJournal = returnModel.SelectedItem.Item;
                    var labelsByConsignors = _dataBaseAdapter.GetMarkedCodesByConsignors(docJournal.Id);

                    int consignorsCount = 0;
                    foreach (var labelsByConsignor in labelsByConsignors)
                    {
                        var receiverInn = labelsByConsignor.Key;

                        foreach (var labelsBySender in labelsByConsignor.Value)
                        {
                            string orgInn = labelsBySender.Key;

                            if (string.IsNullOrEmpty(orgInn))
                                throw new Exception("Не указан ИНН организации.");

                            var myOrganization = MyOrganizations.FirstOrDefault(o => o.OrgInn == orgInn);

                            if (myOrganization == null)
                                throw new Exception($"Не найдена организация с ИНН {orgInn}.");

                            var honestMarkSystem = myOrganization.HonestMarkSystem;
                            var edoSystem = myOrganization.EdoSystem;

                            if (!(honestMarkSystem?.Authorization() ?? false))
                                throw new Exception($"Авторизация организации {orgInn} в Честном знаке не была успешной");

                            if(!(edoSystem?.Authorization() ?? false))
                                throw new Exception($"Авторизация организации {orgInn} в веб сервисе ЭДО не была успешной.");

                            var productList = new List<Reporter.Entities.Product>();
                            loadContext.SetLoadingText("Проверка кодов");
                            int i = 0;
                            foreach (var detail in docJournal.Details)
                            {
                                var barCode = _dataBaseAdapter?.GetBarCodeByIdGood(detail.IdGood);

                                var product = new Reporter.Entities.Product()
                                {
                                    Number = ++i,
                                    Description = detail?.Good?.Name,
                                    UnitCode = "796",
                                    Quantity = detail.Quantity,
                                    Price = (decimal)Math.Round(detail.Price - detail.DiscountSumm, 2),
                                    TaxAmount = 0,
                                    BarCode = barCode,
                                    UnitName = "шт"
                                };

                                var refGood = _dataBaseAdapter.GetRefGoodById(detail.IdGood) as RefGood;

                                product.OriginCode = refGood?.Country?.NumCode?.ToString();
                                product.OriginCountryName = refGood?.Country?.Name?.ToString();

                                if (!string.IsNullOrEmpty(refGood?.CustomsNo))
                                    product.CustomsDeclarationCode = refGood?.CustomsNo;

                                var refGoodsByBarCode = _dataBaseAdapter.GetRefGoodsByBarCode(barCode)?.Cast<RefGood>() ?? new List<RefGood>();

                                product.MarkedCodes = labelsBySender.Value?.Cast<DocGoodsDetailsLabels>()?
                                .Where(l => refGoodsByBarCode.Any(r => r.Id == l.IdGood))?
                                .Select(l => l.DmLabel)?.ToList() ?? new List<string>();

                                if (product.MarkedCodes.Count != product.Quantity)
                                    throw new Exception("Количество кодов маркировки не совпадает с количеством товара.");

                                productList.Add(product);
                            }

                            var markedCodes = productList?.SelectMany(p => p.MarkedCodes) ?? new List<string>();

                            if (markedCodes.Count() == 0)
                            {
                                loadActionContext.ErrorMessage = "В документе отсутствуют коды маркировки для оформления возврата";
                                loadActionContext.TitleErrorText = "Нет кодов маркировки для возврата.";
                                return;
                            }
                            else
                            {
                                if (honestMarkSystem != null && !MarkedCodesOwnerCheck(markedCodes, orgInn))
                                {
                                    loadActionContext.ErrorMessage = "В списке кодов маркировки есть не принадлежащие организации.";
                                    loadActionContext.TitleErrorText = "Произошла ошибка проверки кодов.";
                                    return;
                                }
                            }

                            var cryptoUtil = new UtilitesLibrary.Service.CryptoUtil();
                            var certs = cryptoUtil.GetPersonalCertificates().OrderByDescending(c => c.NotBefore);

                            var client = new System.Net.WebClient();

                            if (ConfigSet.Configs.Config.GetInstance().ProxyEnabled)
                            {
                                var webProxy = new System.Net.WebProxy();

                                webProxy.Address = new Uri("http://" + ConfigSet.Configs.Config.GetInstance().ProxyAddress);
                                webProxy.Credentials = new System.Net.NetworkCredential(ConfigSet.Configs.Config.GetInstance().ProxyUserName,
                                    ConfigSet.Configs.Config.GetInstance().ProxyUserPassword);

                                client.Proxy = webProxy;
                            }

                            cryptoUtil = new UtilitesLibrary.Service.CryptoUtil(client);

                            var receiverCert = certs?.FirstOrDefault(c => cryptoUtil.GetOrgInnFromCertificate(c) == receiverInn
                            && cryptoUtil.IsCertificateValid(c) && c.NotAfter > DateTime.Now);

                            if (receiverCert == null)
                                throw new Exception("Не найден сертификат по ИНН организации.");

                            cryptoUtil = new UtilitesLibrary.Service.CryptoUtil(receiverCert);

                            object[] parameters = null;

                            if (edoSystem as DiadocEdoSystem != null)
                            {
                                var diadocEdoSystem = edoSystem as DiadocEdoSystem;
                                var orgId = diadocEdoSystem.GetMyOrgId(orgInn);

                                parameters = new[] { orgId };
                            }
                            else if (edoSystem as EdoLiteSystem != null)
                                parameters = new[] { honestMarkSystem };

                            var senderEdoId = edoSystem.GetOrganizationEdoIdByInn(orgInn, myOrganization.OrgInn, parameters);
                            var receiverEdoId = edoSystem.GetOrganizationEdoIdByInn(receiverInn, myOrganization.OrgInn, parameters);

                            var orgName = myOrganization.CryptoUtil.ParseCertAttribute(edoSystem.GetCertSubject(), "CN").Replace("\"\"", "\"").Replace("\"\"", "\"").TrimStart('"');

                            loadContext.SetLoadingText("Формирование УПД");
                            var sellerReport = new Reporter.Reports.UniversalTransferSellerDocument();

                            sellerReport.Products = productList;

                            sellerReport.EdoProgramVersion = EdoProgramVersion;
                            sellerReport.FileName = $"ON_NSCHFDOPPRMARK_{receiverEdoId}_{senderEdoId}_{DateTime.Now.ToString("yyyyMMdd")}_{Guid.NewGuid().ToString()}";

                            sellerReport.EdoId = edoSystem.EdoId;
                            sellerReport.SenderEdoId = senderEdoId;
                            sellerReport.ReceiverEdoId = receiverEdoId;

                            sellerReport.EdoProviderOrgName = edoSystem.EdoOrgName;
                            sellerReport.ProviderInn = edoSystem.EdoOrgInn;

                            sellerReport.CreateDate = DateTime.Now;
                            sellerReport.FinSubjectCreator = $"{orgName}, ИНН: {orgInn}";
                            sellerReport.Function = "ДОП";
                            sellerReport.EconomicLifeDocName = "Документ об отгрузке товаров (выполнении работ), передаче имущественных прав (документ об оказании услуг)";
                            sellerReport.DocName = "Документ об отгрузке товаров (выполнении работ), передаче имущественных прав (документ об оказании услуг)";

                            if (consignorsCount == 0)
                                sellerReport.DocNumber = docJournal.Code;
                            else
                                sellerReport.DocNumber = $"{docJournal.Code}-0{consignorsCount}";

                            consignorsCount++;

                            sellerReport.DocDate = DateTime.Now.Date;
                            sellerReport.CurrencyCode = "643";

                            string receiverOrgName = null;
                            string receiverEmchdId = null;
                            if (receiverInn.Length == 10)
                            {
                                var receiverCompany = _dataBaseAdapter.GetCustomerByOrgInn(receiverInn) as RefCustomer;

                                if (receiverCompany == null)
                                    throw new Exception("Для получателя не найдена компания в системе.");

                                var refAuthoritySignDocuments = _dataBaseAdapter.GetRefAuthoritySignDocumentsByCustomer(receiverCompany.Id) as RefAuthoritySignDocuments;

                                if (refAuthoritySignDocuments != null)
                                    receiverEmchdId = refAuthoritySignDocuments.EmchdId;

                                var buyerOrganizationExchangeParticipant = new Reporter.Entities.OrganizationExchangeParticipantEntity();

                                buyerOrganizationExchangeParticipant.JuridicalInn = receiverInn;
                                buyerOrganizationExchangeParticipant.JuridicalKpp = receiverCompany.Kpp;
                                buyerOrganizationExchangeParticipant.OrgName = receiverCompany.Name;

                                sellerReport.BuyerEntity = buyerOrganizationExchangeParticipant;
                                receiverOrgName = buyerOrganizationExchangeParticipant.OrgName;
                            }
                            else if (receiverInn.Length == 12)
                            {
                                var buyerJuridicalEntity = new Reporter.Entities.JuridicalEntity();
                                buyerJuridicalEntity.Inn = receiverInn;

                                buyerJuridicalEntity.Surname = cryptoUtil.ParseCertAttribute(receiverCert.Subject, "SN");

                                var firstMiddleName = cryptoUtil.ParseCertAttribute(receiverCert.Subject, "G");
                                buyerJuridicalEntity.Name = firstMiddleName.IndexOf(" ") > 0 ? firstMiddleName.Substring(0, firstMiddleName.IndexOf(" ")) : string.Empty;
                                buyerJuridicalEntity.Patronymic = firstMiddleName.IndexOf(" ") >= 0 && firstMiddleName.Length > firstMiddleName.IndexOf(" ") + 1 ? firstMiddleName.Substring(firstMiddleName.IndexOf(" ") + 1) : string.Empty;

                                sellerReport.BuyerEntity = buyerJuridicalEntity;
                                receiverOrgName = $"ИП {buyerJuridicalEntity.Surname} {buyerJuridicalEntity.Name} {buyerJuridicalEntity.Patronymic}";
                            }

                            sellerReport.BuyerAddress = new Reporter.Entities.Address
                            {
                                CountryCode = "643",
                                RussianRegionCode = receiverInn.Substring(0, 2),
                                RussianStreet = cryptoUtil.ParseCertAttribute(receiverCert.Subject, "STREET")
                            };

                            string sellerOrgName = null;
                            string sellerEmchdId = null;
                            if (orgInn.Length == 10)
                            {
                                var sellerCompany = _dataBaseAdapter.GetCustomerByOrgInn(orgInn) as RefCustomer;

                                if (sellerCompany == null)
                                    throw new Exception("Для получателя не найдена компания в системе.");

                                var refAuthoritySignDocuments = _dataBaseAdapter.GetRefAuthoritySignDocumentsByCustomer(sellerCompany.Id) as RefAuthoritySignDocuments;

                                if (refAuthoritySignDocuments != null)
                                    sellerEmchdId = refAuthoritySignDocuments.EmchdId;

                                var sellerOrganizationExchangeParticipant = new Reporter.Entities.OrganizationExchangeParticipantEntity();

                                sellerOrganizationExchangeParticipant.JuridicalInn = sellerCompany.Inn;
                                sellerOrganizationExchangeParticipant.JuridicalKpp = sellerCompany.Kpp;
                                sellerOrganizationExchangeParticipant.OrgName = sellerCompany.Name;

                                sellerReport.SellerEntity = sellerOrganizationExchangeParticipant;
                                sellerOrgName = sellerOrganizationExchangeParticipant.OrgName;
                            }
                            else if (orgInn.Length == 12)
                            {
                                var sellerJuridicalEntity = new Reporter.Entities.JuridicalEntity();
                                sellerJuridicalEntity.Inn = orgInn;

                                sellerJuridicalEntity.Surname = myOrganization.CryptoUtil.ParseCertAttribute(edoSystem.GetCertSubject(), "SN");
                                var firstMiddleName = myOrganization.CryptoUtil.ParseCertAttribute(edoSystem.GetCertSubject(), "G");
                                sellerJuridicalEntity.Name = firstMiddleName.IndexOf(" ") > 0 ? firstMiddleName.Substring(0, firstMiddleName.IndexOf(" ")) : string.Empty;
                                sellerJuridicalEntity.Patronymic = firstMiddleName.IndexOf(" ") >= 0 && firstMiddleName.Length > firstMiddleName.IndexOf(" ") + 1 ? firstMiddleName.Substring(firstMiddleName.IndexOf(" ") + 1) : string.Empty;

                                sellerReport.SellerEntity = sellerJuridicalEntity;
                                sellerOrgName = $"ИП {sellerJuridicalEntity.Surname} {sellerJuridicalEntity.Name} {sellerJuridicalEntity.Patronymic}";
                            }

                            sellerReport.SellerAddress = new Reporter.Entities.Address
                            {
                                CountryCode = "643",
                                RussianRegionCode = orgInn.Substring(0, 2)
                            };

                            sellerReport.CurrencyName = "Российский рубль";
                            sellerReport.DeliveryDocuments = new List<Reporter.Entities.DeliveryDocument>
                {
                    new Reporter.Entities.DeliveryDocument
                    {
                        DocumentName = "Реализация (акт, накладная, УПД)",
                        DocumentNumber = $"п/п 1-{sellerReport.Products.Count}, №{sellerReport.DocNumber}",
                        DocumentDate = DateTime.Now.Date
                    }
                };

                            sellerReport.ContentOperation = "Товары переданы";
                            sellerReport.ShippingDate = DateTime.Now.Date;
                            sellerReport.BasisDocumentName = "Без документа-основания";

                            sellerReport.ScopeOfAuthority = Reporter.Enums.SellerScopeOfAuthorityEnum.PersonWhoResponsibleForRegistrationExecutionAndSigning;
                            sellerReport.SignerStatus = Reporter.Enums.SellerSignerStatusEnum.EmployeeOfSellerOrganization;
                            if (orgInn.Length == 10)
                            {
                                var sellerOrganizationExchangeParticipant = sellerReport.SellerEntity as Reporter.Entities.OrganizationExchangeParticipantEntity;

                                sellerReport.JuridicalInn = sellerOrganizationExchangeParticipant?.JuridicalInn;
                                sellerReport.SignerOrgName = sellerOrganizationExchangeParticipant?.OrgName;

                                sellerReport.SignerPosition = myOrganization.CryptoUtil.ParseCertAttribute(edoSystem.GetCertSubject(), "T");

                                sellerReport.SignerSurname = myOrganization.CryptoUtil.ParseCertAttribute(edoSystem.GetCertSubject(), "SN");
                                var firstMiddleName = myOrganization.CryptoUtil.ParseCertAttribute(edoSystem.GetCertSubject(), "G");
                                sellerReport.SignerName = firstMiddleName.IndexOf(" ") > 0 ? firstMiddleName.Substring(0, firstMiddleName.IndexOf(" ")) : string.Empty;
                                sellerReport.SignerPatronymic = firstMiddleName.IndexOf(" ") >= 0 && firstMiddleName.Length > firstMiddleName.IndexOf(" ") + 1 ? firstMiddleName.Substring(firstMiddleName.IndexOf(" ") + 1) : string.Empty;
                            }
                            else if (orgInn.Length == 12)
                            {
                                sellerReport.SignerEntity = new Reporter.Entities.JuridicalEntity();
                                var sellerEntity = sellerReport.SellerEntity as Reporter.Entities.JuridicalEntity;

                                ((Reporter.Entities.JuridicalEntity)sellerReport.SignerEntity).Inn = sellerEntity.Inn;
                                ((Reporter.Entities.JuridicalEntity)sellerReport.SignerEntity).Surname = sellerEntity.Surname;
                                ((Reporter.Entities.JuridicalEntity)sellerReport.SignerEntity).Name = sellerEntity.Name;
                                ((Reporter.Entities.JuridicalEntity)sellerReport.SignerEntity).Patronymic = sellerEntity.Patronymic;
                            }

                            var sellerXmlContent = sellerReport.GetXmlContent();

                            var sellerFileBytes = Encoding.GetEncoding(1251).GetBytes(sellerXmlContent);
                            var sellerSignature = myOrganization.CryptoUtil.Sign(sellerFileBytes, true);

                            string localPath = string.Empty;

                            loadContext.SetLoadingText("Отправка УПД");
                            if (edoSystem as DiadocEdoSystem != null)
                            {
                                var orgId = parameters[0] as string;

                                parameters = new object[] { orgId, receiverInn, "ДОП", null, null };
                            }
                            else if (edoSystem as EdoLiteSystem != null)
                            {
                                File.WriteAllBytes($"{edoFilesPath}//{sellerReport.FileName}.xml", sellerFileBytes);

                                if (string.IsNullOrEmpty(localPath))
                                {
                                    var directory = new DirectoryInfo(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location));
                                    localPath = directory.Name;
                                    while (directory.Parent != null)
                                    {
                                        directory = directory.Parent;

                                        if (directory.Parent == null)
                                            localPath = $"{directory.Name.Replace(":\\", ":")}/{localPath}";
                                        else
                                            localPath = $"{directory.Name}/{localPath}";
                                    }
                                }

                                string content = $"{localPath}/{edoFilesPath}/{sellerReport.FileName}.xml";
                                parameters = new object[] { content };
                            }

                            object sendSellerReportResult = edoSystem.SendUniversalTransferDocument(sellerFileBytes, sellerSignature, parameters);

                            if (edoSystem as DiadocEdoSystem != null)
                            {
                                var sellerMessage = sendSellerReportResult as Diadoc.Api.Proto.Events.Message;

                                if (!Directory.Exists($"{edoFilesPath}//{sellerMessage.MessageId}"))
                                    Directory.CreateDirectory($"{edoFilesPath}//{sellerMessage.MessageId}");

                                File.WriteAllBytes($"{edoFilesPath}//{sellerMessage.MessageId}//{sellerReport.FileName}.xml", sellerFileBytes);
                                File.WriteAllBytes($"{edoFilesPath}//{sellerMessage.MessageId}//{sellerReport.FileName}.xml.sig", sellerSignature);
                            }
                            else if (edoSystem as EdoLiteSystem != null)
                            {
                                if (File.Exists($"{edoFilesPath}//{sellerReport.FileName}.xml"))
                                    File.Delete($"{edoFilesPath}//{sellerReport.FileName}.xml");

                                var docId = sendSellerReportResult as string;

                                if (!Directory.Exists($"{edoFilesPath}//{docId}"))
                                    Directory.CreateDirectory($"{edoFilesPath}//{docId}");

                                File.WriteAllBytes($"{edoFilesPath}//{docId}//{sellerReport.FileName}.xml", sellerFileBytes);
                                File.WriteAllBytes($"{edoFilesPath}//{docId}//{sellerReport.FileName}.xml.sig", sellerSignature);
                            }

                            loadContext.SetLoadingText("Формирование УПД покупателя");
                            var buyerReport = new Reporter.Reports.UniversalTransferBuyerDocument();
                            buyerReport.BasisOfAuthority = cryptoUtil.ParseCertAttribute(receiverCert.Subject, "T");
                            buyerReport.CreateBuyerFileDate = DateTime.Now;
                            buyerReport.ScopeOfAuthority = Reporter.Enums.ScopeOfAuthorityEnum.PersonWhoMadeOperation;
                            buyerReport.SignerStatus = Reporter.Enums.SignerStatusEnum.Individual;
                            buyerReport.AcceptResult = Reporter.Enums.AcceptResultEnum.GoodsAcceptedWithoutDiscrepancy;
                            buyerReport.SellerFileId = sellerReport.FileName;
                            buyerReport.EdoProviderOrgName = sellerReport.EdoProviderOrgName;
                            buyerReport.ProviderInn = sellerReport.ProviderInn;
                            buyerReport.EdoId = edoSystem.EdoId;
                            buyerReport.SenderEdoId = receiverEdoId;
                            buyerReport.ReceiverEdoId = senderEdoId;
                            buyerReport.CreateSellerFileDate = sellerReport.CreateDate;
                            buyerReport.DocName = sellerReport.DocName;
                            buyerReport.Function = sellerReport.Function;
                            buyerReport.SellerInvoiceNumber = sellerReport.DocNumber;
                            buyerReport.SellerInvoiceDate = sellerReport.DocDate;
                            buyerReport.Signature = Convert.ToBase64String(sellerSignature);
                            buyerReport.DateReceive = DateTime.Now;

                            if (receiverInn.Length == 10)
                            {
                                buyerReport.JuridicalInn = receiverInn;
                                buyerReport.SignerSurname = cryptoUtil.ParseCertAttribute(receiverCert.Subject, "SN");

                                var firstMiddleName = cryptoUtil.ParseCertAttribute(receiverCert.Subject, "G");
                                buyerReport.SignerName = firstMiddleName.IndexOf(" ") > 0 ? firstMiddleName.Substring(0, firstMiddleName.IndexOf(" ")) : string.Empty;
                                buyerReport.SignerPatronymic = firstMiddleName.IndexOf(" ") >= 0 && firstMiddleName.Length > firstMiddleName.IndexOf(" ") + 1 ? firstMiddleName.Substring(firstMiddleName.IndexOf(" ") + 1) : string.Empty;

                                buyerReport.SignerPosition = buyerReport.BasisOfAuthority;
                                buyerReport.SignerOrgName = (sellerReport.BuyerEntity as Reporter.Entities.OrganizationExchangeParticipantEntity)?.OrgName;
                                buyerReport.FinSubjectCreator = $"{buyerReport.SignerOrgName}, ИНН: {receiverInn}";
                            }
                            else if (receiverInn.Length == 12)
                            {
                                buyerReport.SignerEntity = sellerReport.BuyerEntity;
                                var buyerOrgName = cryptoUtil.ParseCertAttribute(receiverCert.Subject, "CN").Replace("\"\"", "\"").Replace("\"\"", "\"").TrimStart('"');
                                buyerReport.FinSubjectCreator = $"{buyerOrgName}, ИНН: {receiverInn}";
                            }

                            buyerReport.FileName = $"ON_NSCHFDOPPOKMARK_{senderEdoId}_{receiverEdoId}_{DateTime.Now.ToString("yyyyMMdd")}_{Guid.NewGuid().ToString()}";
                            var buyerXmlContent = buyerReport.GetXmlContent();

                            var buyerFileBytes = Encoding.GetEncoding(1251).GetBytes(buyerXmlContent);
                            var buyerSignature = cryptoUtil.Sign(buyerFileBytes, true);

                            if (edoSystem as DiadocEdoSystem != null)
                            {
                                var sellerMessage = sendSellerReportResult as Diadoc.Api.Proto.Events.Message;

                                if (!Directory.Exists($"{edoFilesPath}//{sellerMessage.MessageId}"))
                                    Directory.CreateDirectory($"{edoFilesPath}//{sellerMessage.MessageId}");

                                File.WriteAllBytes($"{edoFilesPath}//{sellerMessage.MessageId}//{buyerReport.FileName}.xml", buyerFileBytes);
                                File.WriteAllBytes($"{edoFilesPath}//{sellerMessage.MessageId}//{buyerReport.FileName}.xml.sig", buyerSignature);
                            }
                            else if (edoSystem as EdoLiteSystem != null)
                            {
                                var docId = sendSellerReportResult as string;

                                if (!Directory.Exists($"{edoFilesPath}//{docId}"))
                                    Directory.CreateDirectory($"{edoFilesPath}//{docId}");

                                File.WriteAllBytes($"{edoFilesPath}//{docId}//{buyerReport.FileName}.xml", buyerFileBytes);
                                File.WriteAllBytes($"{edoFilesPath}//{docId}//{buyerReport.FileName}.xml.sig", buyerSignature);
                            }

                            loadContext.SetLoadingText("Отправка УПД покупателя");
                            if (edoSystem as DiadocEdoSystem != null)
                            {
                                var sellerMessage = sendSellerReportResult as Diadoc.Api.Proto.Events.Message;
                                var entity = sellerMessage.Entities.FirstOrDefault(t => t.AttachmentType == Diadoc.Api.Proto.Events.AttachmentType.UniversalTransferDocument);

                                edoSystem.SendDocument(sellerMessage.MessageId, buyerFileBytes, buyerSignature, receiverEmchdId, entity.EntityId, (int)Diadoc.Api.Proto.DocumentType.UniversalTransferDocumentRevision, sellerMessage.ToBoxId, receiverCert, receiverInn);
                            }
                            else if (edoSystem as EdoLiteSystem != null)
                            {
                                var docId = sendSellerReportResult as string;

                                if (string.IsNullOrEmpty(localPath))
                                {
                                    var directory = new DirectoryInfo(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location));
                                    localPath = directory.Name;
                                    while (directory.Parent != null)
                                    {
                                        directory = directory.Parent;

                                        if (directory.Parent == null)
                                            localPath = $"{directory.Name.Replace(":\\", ":")}/{localPath}";
                                        else
                                            localPath = $"{directory.Name}/{localPath}";
                                    }
                                }

                                string content = $"{localPath}/{edoFilesPath}/{docId}/{buyerReport.FileName}.xml";
                                edoSystem.SendDocument(docId, buyerFileBytes, buyerSignature, receiverEmchdId, content);
                            }

                            using (var transaction = _dataBaseAdapter.BeginTransaction())
                            {
                                try
                                {
                                    if (edoSystem as DiadocEdoSystem != null)
                                    {
                                        var sellerMessage = sendSellerReportResult as Diadoc.Api.Proto.Events.Message;
                                        var entity = sellerMessage.Entities.FirstOrDefault(t => t.AttachmentType == Diadoc.Api.Proto.Events.AttachmentType.UniversalTransferDocument);
                                        _dataBaseAdapter.AddDocEdoReturnPurchasing(docJournal.Id, sellerMessage.MessageId, entity.EntityId, sellerReport.FileName, buyerReport.FileName,
                                            orgInn, sellerOrgName, receiverInn, receiverOrgName, DateTime.Now);
                                    }
                                    else if (edoSystem as EdoLiteSystem != null)
                                    {
                                        var docId = sendSellerReportResult as string;
                                        _dataBaseAdapter.AddDocEdoReturnPurchasing(docJournal.Id, docId, null, sellerReport.FileName, buyerReport.FileName,
                                            orgInn, sellerOrgName, receiverInn, receiverOrgName, DateTime.Now);
                                    }
                                    else
                                        _dataBaseAdapter.AddDocEdoReturnPurchasing(docJournal.Id, null, null, sellerReport.FileName, buyerReport.FileName,
                                            orgInn, sellerOrgName, receiverInn, receiverOrgName, DateTime.Now);

                                    foreach (var label in labelsBySender.Value?.Cast<DocGoodsDetailsLabels>() ?? new List<DocGoodsDetailsLabels>())
                                        label.IdDocSale = null;

                                    _dataBaseAdapter.Commit(transaction);
                                }
                                catch (Exception ex)
                                {
                                    transaction.Rollback();
                                    throw ex;
                                }
                            }
                        }
                    }
                    loadContext.SetSuccessFullLoad("Процесс завершён успешно");
                }
                catch (System.Net.WebException webEx)
                {
                    //transaction.Rollback();
                    loadActionContext.ErrorMessage = _log.GetRecursiveInnerException(webEx);
                    loadActionContext.TitleErrorText = "Произошла ошибка возврата кодов на удалённом сервере.";
                }
                catch (Exception ex)
                {
                    //transaction.Rollback();
                    loadActionContext.ErrorMessage = _log.GetRecursiveInnerException(ex);
                    loadActionContext.TitleErrorText = "Произошла ошибка возврата кодов.";
                }
            };

            returnWindow.ShowDialog();
            SelectedMyOrganization = null;
            this.ChangeMyOrganization();
        }

        private void WithdrawalCodes()
        {
            if (SelectedMyOrganization == null)
            {
                System.Windows.MessageBox.Show(
                    "Не выбрана организация для вывода кодов из оборота.", "Ошибка", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                return;
            }

            if (SelectedMyOrganization.HonestMarkSystem == null)
            {
                System.Windows.MessageBox.Show(
                    "Невозможно оформить вывод из оборота.\nНе пройдена авторизация в Честном знаке.", "Ошибка", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                return;
            }

            var markedCodes = _dataBaseAdapter.GetAllMarkedCodes().Cast<DocGoodsDetailsLabels>().ToList();

            var withdrawalWindow = new ChangeMarkedCodesWindow();
            withdrawalWindow.DataContext = new ChangeMarkedCodesModel(markedCodes, SelectedMyOrganization);
            var permittedProductGroups = _dataBaseAdapter.GetHonestMarkProductGroups().Cast<RefHonestMarkProductGroup>();
            var productGroupEnumValues = typeof(ProductGroupsEnum).GetEnumValues().OfType<ProductGroupsEnum>();

            (withdrawalWindow.DataContext as ChangeMarkedCodesModel).AllProductGroups = permittedProductGroups.Select(p =>
            {
                var enumValue = productGroupEnumValues.First(e => (int)e == p.Id);
                return new KeyValuePair<ProductGroupsEnum, string>(enumValue, p.Description);
            });

            withdrawalWindow.ShowDialog();
        }

        private async void RejectDocument()
        {
            if (SelectedMyOrganization == null)
            {
                System.Windows.MessageBox.Show(
                    "Не выбрана организация для отклонения документов.", "Ошибка", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                return;
            }

            if (SelectedItem == null)
            {
                System.Windows.MessageBox.Show(
                    "Не выбран документ для отклонения.", "Ошибка", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                return;
            }

            if (SelectedItem.DocStatus == (int)DocEdoStatus.Rejected)
            {
                System.Windows.MessageBox.Show(
                    "Документ уже был отклонён.", "Ошибка", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                return;
            }

            if (SelectedItem.DocStatus != (int)DocEdoStatus.New)
            {
                System.Windows.MessageBox.Show(
                    "Невозможно отклонить документ в данном статусе.", "Ошибка", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                return;
            }

            if (SelectedMyOrganization.EdoSystem == null)
            {
                System.Windows.MessageBox.Show(
                    "Регистрация в веб сервисе не была успешной.", "Ошибка", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                return;
            }

            var rejectWindow = new RejectWindow();
            if (rejectWindow.ShowDialog() == true)
            {
                string errorMessage = null;
                string titleErrorText = null;

                LoadWindow loadWindow = new LoadWindow("Подождите, идёт загрузка данных");

                if (this.Owner != null)
                    loadWindow.Owner = this.Owner;

                var loadContext = loadWindow.GetLoadContext();
                var report = rejectWindow.Report;
                var edoSystem = SelectedMyOrganization.EdoSystem;
                var cryptoUtil = SelectedMyOrganization.CryptoUtil;

                loadWindow.Show();
                await Task.Run(() =>
                {
                    using (var transaction = _dataBaseAdapter.BeginTransaction())
                    {
                        try
                        {
                            loadContext.SetLoadingText("Формирование файла отказа");
                            var sellerXmlDocument = new XmlDocument();
                            sellerXmlDocument.Load($"{edoFilesPath}//{SelectedItem.IdDocEdo}//{SelectedItem.FileName}.xml");
                            var docSellerContent = Encoding.GetEncoding(1251).GetBytes(sellerXmlDocument.OuterXml);

                            var reporterDll = new Reporter.ReporterDll();
                            var sellerReport = reporterDll.ParseDocument<Reporter.Reports.UniversalTransferSellerDocument>(docSellerContent);

                            report.FileName = $"DP_UVUTOCH_{sellerReport.SenderEdoId}_{sellerReport.ReceiverEdoId}_{DateTime.Now.ToString("yyyyMMdd")}_{Guid.NewGuid().ToString()}";
                            report.EdoProgramVersion = this.EdoProgramVersion;

                            report.CreatorEdoId = sellerReport.ReceiverEdoId;

                            if (SelectedItem.ReceiverInn.Length == 10)
                            {
                                report.JuridicalInn = SelectedItem.ReceiverInn;
                                report.JuridicalKpp = SelectedItem.ReceiverKpp;
                                report.OrgCreatorName = SelectedItem.ReceiverName;
                            }

                            report.ReceiveDate = DateTime.Now;
                            report.ReceivedFileName = sellerReport.FileName;

                            report.SenderEdoId = sellerReport.SenderEdoId;
                            report.SenderJuridicalInn = SelectedItem.SenderInn;
                            report.SenderJuridicalKpp = SelectedItem.SenderKpp;
                            report.OrgSenderName = SelectedItem.SenderName;

                            var subject = edoSystem.GetCertSubject();
                            var firstMiddleName = cryptoUtil.ParseCertAttribute(subject, "G");
                            report.SignerPosition = cryptoUtil.ParseCertAttribute(subject, "T");
                            report.SignerSurname = cryptoUtil.ParseCertAttribute(subject, "SN");
                            report.SignerName = firstMiddleName.IndexOf(" ") > 0 ? firstMiddleName.Substring(0, firstMiddleName.IndexOf(" ")) : string.Empty;
                            report.SignerPatronymic = firstMiddleName.IndexOf(" ") >= 0 && firstMiddleName.Length > firstMiddleName.IndexOf(" ") + 1 ? firstMiddleName.Substring(firstMiddleName.IndexOf(" ") + 1) : string.Empty;

                            if (string.IsNullOrEmpty(report.SignerPosition))
                                report.SignerPosition = "Сотрудник с правом подписи";

                            if (SelectedItem.ReceiverInn.Length == 12)
                            {
                                report.IndividualCreator = new Reporter.Entities.IndividualEntity
                                {
                                    Inn = SelectedItem.ReceiverInn,
                                    Surname = report.SignerSurname,
                                    Name = report.SignerName,
                                    Patronymic = report.SignerPatronymic
                                };
                            }

                            string signedFilePath;

                            if (edoSystem.HasZipContent)
                            {
                                using (ZipArchive zipArchive = ZipFile.Open($"{edoFilesPath}//{SelectedItem.IdDocEdo}//{SelectedItem.FileName}.zip", ZipArchiveMode.Read))
                                {
                                    var signedEntry = zipArchive.Entries.FirstOrDefault(x => x.Name.StartsWith(SelectedItem.FileName) && x.Name != $"{SelectedItem.FileName}.xml");

                                    if (signedEntry == null)
                                        throw new Exception("Не найден файл подписи продавца.");

                                    if (!File.Exists($"{edoFilesPath}//{SelectedItem.IdDocEdo}//{signedEntry.Name}"))
                                        signedEntry?.ExtractToFile($"{edoFilesPath}//{SelectedItem.IdDocEdo}//{signedEntry.Name}");

                                    signedFilePath = $"{edoFilesPath}//{SelectedItem.IdDocEdo}//{signedEntry.Name}";
                                }

                            }
                            else
                            {
                                signedFilePath = $"{edoFilesPath}//{SelectedItem.IdDocEdo}//{SelectedItem.FileName}.xml.sig";

                                if (!File.Exists(signedFilePath))
                                {
                                    System.Windows.MessageBox.Show("Не найден файл подписи продавца.", "Ошибка", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                                    return;
                                }
                            }

                            var sellerSignature = File.ReadAllBytes(signedFilePath);
                            report.ReceivedFileSignature = Convert.ToBase64String(sellerSignature);

                            loadContext.SetLoadingText("Сохранение документов");
                            var xmlContent = report.GetXmlContent();
                            var contentBytes = Encoding.GetEncoding(1251).GetBytes(xmlContent);
                            var signature = cryptoUtil.Sign(contentBytes, true);

                            File.WriteAllBytes($"{edoFilesPath}//{SelectedItem.IdDocEdo}//{report.FileName}.xml", contentBytes);
                            File.WriteAllBytes($"{edoFilesPath}//{SelectedItem.IdDocEdo}//{report.FileName}.xml.sig", signature);

                            loadContext.SetLoadingText("Отправка");

                            if(edoSystem.GetType() == typeof(DiadocEdoSystem))
                                edoSystem.SendRejectionDocument(sellerReport.Function, contentBytes, signature, SelectedItem.IdDocEdo, SelectedItem.ParentEntityId);

                            SelectedItem.DocStatus = (int)DocEdoStatus.Rejected;

                            _dataBaseAdapter.Commit(transaction);
                            loadContext.SetSuccessFullLoad("Документ успешно отклонён.");
                        }
                        catch (System.Net.WebException webEx)
                        {
                            transaction.Rollback();
                            _dataBaseAdapter.ReloadEntry(SelectedItem);
                            errorMessage = _log.GetRecursiveInnerException(webEx);
                            titleErrorText = "Произошла ошибка отклонения документа на удалённом сервере.";
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            _dataBaseAdapter.ReloadEntry(SelectedItem);
                            errorMessage = _log.GetRecursiveInnerException(ex);
                            titleErrorText = "Произошла ошибка отклонения документа.";
                        }
                    }
                });

                if (!string.IsNullOrEmpty(errorMessage))
                {
                    loadWindow.Close();
                    _log.Log(errorMessage);
                    var errorsWindow = new ErrorsWindow(titleErrorText, new List<string>(new string[] { errorMessage }));
                    errorsWindow.ShowDialog();
                }
            }
        }

        private async void RevokeDocument()
        {
            if (SelectedMyOrganization == null)
            {
                System.Windows.MessageBox.Show(
                    "Не выбрана организация для аннулирования документов.", "Ошибка", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                return;
            }

            if (SelectedItem == null)
            {
                System.Windows.MessageBox.Show(
                    "Не выбран документ для аннулирования.", "Ошибка", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                return;
            }

            if (SelectedItem.DocStatus == (int)DocEdoStatus.Revoked)
            {
                System.Windows.MessageBox.Show(
                    "Документ уже был аннулирован.", "Ошибка", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                return;
            }

            if (SelectedMyOrganization.EdoSystem == null)
            {
                System.Windows.MessageBox.Show(
                    "Регистрация в веб сервисе не была успешной.", "Ошибка", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                return;
            }

            var edoSystem = SelectedMyOrganization.EdoSystem;
            var cryptoUtil = SelectedMyOrganization.CryptoUtil;
            LoadWindow loadWindow = new LoadWindow("Подождите, идёт загрузка данных");

            if (SelectedItem.DocStatus == (int)DocEdoStatus.RevokeRequired)
            {
                var comfirmRevokeWindow = new ConfirmRevokeWindow();
                string fileName = null;

                try
                {
                    Exception exception = null;

                    if (this.Owner != null)
                        loadWindow.Owner = this.Owner;

                    comfirmRevokeWindow.ShowDialog();

                    if (comfirmRevokeWindow.Result == RevokeRequestDialogResult.None)
                        return;

                    if (edoSystem.GetType() == typeof(DiadocEdoSystem))
                    {
                        loadWindow.Show();
                        byte[] sellerSignature;
                        var revokeDocumentEntity = (Diadoc.Api.Proto.Events.Entity)edoSystem.GetRevokeDocument(out fileName, out sellerSignature, SelectedItem.CounteragentEdoBoxId, SelectedItem.IdDocEdo, SelectedItem.ParentEntityId);
                        var revokeDocumentContent = revokeDocumentEntity?.Content?.Data;

                        if (revokeDocumentContent == null)
                            throw new Exception("Не удалось загрузить документ Запрос на аннулирование.");

                        if (comfirmRevokeWindow.Result == RevokeRequestDialogResult.Revoke)
                        {
                            var signature = cryptoUtil.Sign(revokeDocumentContent, true);

                            File.WriteAllBytes($"{edoFilesPath}//{SelectedItem.IdDocEdo}//{fileName}", revokeDocumentContent);
                            File.WriteAllBytes($"{edoFilesPath}//{SelectedItem.IdDocEdo}//{fileName}.sig", signature);

                            edoSystem.SendRevokeConfirmation(signature, SelectedItem.IdDocEdo, revokeDocumentEntity.EntityId);

                            using (var transaction = _dataBaseAdapter.BeginTransaction())
                            {
                                try
                                {
                                    SelectedItem.DocStatus = (int)DocEdoStatus.Revoked;
                                    _dataBaseAdapter.Commit(transaction);
                                }
                                catch(Exception ex)
                                {
                                    transaction.Rollback();
                                    _dataBaseAdapter.ReloadEntry(SelectedItem);
                                    throw ex;
                                }
                            }
                        }
                        else if(comfirmRevokeWindow.Result == RevokeRequestDialogResult.RejectRevoke)
                        {
                            var report = comfirmRevokeWindow.Report;
                            var loadContext = loadWindow.GetLoadContext();

                            await Task.Run(() => 
                            {
                                using (var transaction = _dataBaseAdapter.BeginTransaction())
                                {
                                    try
                                    {
                                        loadContext.SetLoadingText("Формирование файла отказа");
                                        var sellerXmlDocument = new XmlDocument();
                                        sellerXmlDocument.Load($"{edoFilesPath}//{SelectedItem.IdDocEdo}//{SelectedItem.FileName}.xml");
                                        var docSellerContent = Encoding.GetEncoding(1251).GetBytes(sellerXmlDocument.OuterXml);

                                        var reporterDll = new Reporter.ReporterDll();
                                        var sellerReport = reporterDll.ParseDocument<Reporter.Reports.UniversalTransferSellerDocument>(docSellerContent);

                                        report.FileName = $"DP_UVUTOCH_{sellerReport.SenderEdoId}_{sellerReport.ReceiverEdoId}_{DateTime.Now.ToString("yyyyMMdd")}_{Guid.NewGuid().ToString()}";
                                        report.EdoProgramVersion = this.EdoProgramVersion;

                                        report.CreatorEdoId = sellerReport.ReceiverEdoId;

                                        if (SelectedItem.ReceiverInn.Length == 10)
                                        {
                                            report.JuridicalInn = SelectedItem.ReceiverInn;
                                            report.JuridicalKpp = SelectedItem.ReceiverKpp;
                                            report.OrgCreatorName = SelectedItem.ReceiverName;
                                        }

                                        var fileNameLength = fileName.LastIndexOf('.');

                                        if (fileNameLength < 0)
                                            fileNameLength = fileName.Length;

                                        report.ReceiveDate = DateTime.Now;
                                        report.ReceivedFileName = fileName.Substring(0, fileNameLength);
                                        report.ReceivedFileSignature = Convert.ToBase64String(sellerSignature);

                                        report.SenderEdoId = sellerReport.SenderEdoId;
                                        report.SenderJuridicalInn = SelectedItem.SenderInn;
                                        report.SenderJuridicalKpp = SelectedItem.SenderKpp;
                                        report.OrgSenderName = SelectedItem.SenderName;

                                        var subject = edoSystem.GetCertSubject();
                                        var firstMiddleName = cryptoUtil.ParseCertAttribute(subject, "G");
                                        report.SignerPosition = cryptoUtil.ParseCertAttribute(subject, "T");
                                        report.SignerSurname = cryptoUtil.ParseCertAttribute(subject, "SN");
                                        report.SignerName = firstMiddleName.IndexOf(" ") > 0 ? firstMiddleName.Substring(0, firstMiddleName.IndexOf(" ")) : string.Empty;
                                        report.SignerPatronymic = firstMiddleName.IndexOf(" ") >= 0 && firstMiddleName.Length > firstMiddleName.IndexOf(" ") + 1 ? firstMiddleName.Substring(firstMiddleName.IndexOf(" ") + 1) : string.Empty;

                                        if (string.IsNullOrEmpty(report.SignerPosition))
                                            report.SignerPosition = "Сотрудник с правом подписи";

                                        if (SelectedItem.ReceiverInn.Length == 12)
                                        {
                                            report.IndividualCreator = new Reporter.Entities.IndividualEntity
                                            {
                                                Inn = SelectedItem.ReceiverInn,
                                                Surname = report.SignerSurname,
                                                Name = report.SignerName,
                                                Patronymic = report.SignerPatronymic
                                            };
                                        }

                                        loadContext.SetLoadingText("Сохранение документов");
                                        var xmlContent = report.GetXmlContent();
                                        var contentBytes = Encoding.GetEncoding(1251).GetBytes(xmlContent);
                                        var signature = cryptoUtil.Sign(contentBytes, true);

                                        File.WriteAllBytes($"{edoFilesPath}//{SelectedItem.IdDocEdo}//{report.FileName}.xml", contentBytes);
                                        File.WriteAllBytes($"{edoFilesPath}//{SelectedItem.IdDocEdo}//{report.FileName}.xml.sig", signature);

                                        loadContext.SetLoadingText("Отправка");

                                        edoSystem.SendRejectionDocument(sellerReport.Function, contentBytes, signature, SelectedItem.IdDocEdo, revokeDocumentEntity.EntityId);

                                        SelectedItem.DocStatus = (int)DocEdoStatus.RejectRevoke;
                                        _dataBaseAdapter.Commit(transaction);
                                    }
                                    catch (System.Net.WebException webEx)
                                    {
                                        transaction.Rollback();
                                        _dataBaseAdapter.ReloadEntry(SelectedItem);
                                        exception = webEx;
                                    }
                                    catch (Exception ex)
                                    {
                                        transaction.Rollback();
                                        _dataBaseAdapter.ReloadEntry(SelectedItem);
                                        exception = ex;
                                    }
                                }
                            });

                            if (exception != null)
                                throw exception;
                        }
                    }

                    if (comfirmRevokeWindow.Result == RevokeRequestDialogResult.Revoke && !string.IsNullOrEmpty(fileName))
                    {
                        loadWindow.GetLoadContext().SetSuccessFullLoad("Документ успешно аннулирован.");
                    }
                    else if (comfirmRevokeWindow.Result == RevokeRequestDialogResult.RejectRevoke && !string.IsNullOrEmpty(fileName))
                    {
                        loadWindow.GetLoadContext().SetSuccessFullLoad("В аннулировании отказано.");
                    }
                    
                }
                catch (System.Net.WebException webEx)
                {
                    loadWindow.Close();
                    string errMessage = _log.GetRecursiveInnerException(webEx);
                    _log.Log(errMessage);

                    var errorsWindow = new ErrorsWindow("Произошла ошибка аннулирования документа на удалённом сервере.", new List<string>(new string[] { errMessage }));
                    errorsWindow.ShowDialog();
                }
                catch (Exception ex)
                {
                    loadWindow.Close();
                    string errMessage = _log.GetRecursiveInnerException(ex);
                    _log.Log(errMessage);

                    var errorsWindow = new ErrorsWindow("Произошла ошибка аннулирования документа.", new List<string>(new string[] { errMessage }));
                    errorsWindow.ShowDialog();
                }
            }
            else
            {
                var revokeWindow = new RevokeWindow();
                if(revokeWindow.ShowDialog() == true)
                {
                    string errorMessage = null;
                    string titleErrorText = null;

                    if (this.Owner != null)
                        loadWindow.Owner = this.Owner;

                    var loadContext = loadWindow.GetLoadContext();
                    var report = revokeWindow.Report;

                    loadWindow.Show();
                    await Task.Run(() =>
                    {
                        using (var transaction = _dataBaseAdapter.BeginTransaction())
                        {
                            try
                            {
                                loadContext.SetLoadingText("Формирование файла");
                                var sellerXmlDocument = new XmlDocument();
                                sellerXmlDocument.Load($"{edoFilesPath}//{SelectedItem.IdDocEdo}//{SelectedItem.FileName}.xml");
                                var docSellerContent = Encoding.GetEncoding(1251).GetBytes(sellerXmlDocument.OuterXml);

                                var reporterDll = new Reporter.ReporterDll();
                                var sellerReport = reporterDll.ParseDocument<Reporter.Reports.UniversalTransferSellerDocument>(docSellerContent);

                                report.FileName = $"DP_PRANNUL_{sellerReport.SenderEdoId}_{sellerReport.ReceiverEdoId}_{DateTime.Now.ToString("yyyyMMdd")}_{Guid.NewGuid().ToString()}";
                                report.EdoProgramVersion = this.EdoProgramVersion;

                                report.CreatorEdoId = sellerReport.ReceiverEdoId;

                                if (SelectedItem.ReceiverInn.Length == 10)
                                {
                                    report.JuridicalCreatorInn = SelectedItem.ReceiverInn;
                                    report.JuridicalCreatorKpp = SelectedItem.ReceiverKpp;
                                    report.OrgCreatorName = SelectedItem.ReceiverName;
                                }

                                report.ReceivedFileName = SelectedItem.FileName;

                                report.ReceiverEdoId = sellerReport.SenderEdoId;
                                report.JuridicalReceiverInn = SelectedItem.SenderInn;
                                report.JuridicalReceiverKpp = SelectedItem.SenderKpp;
                                report.OrgReceiverName = SelectedItem.SenderName;

                                var subject = edoSystem.GetCertSubject();
                                var firstMiddleName = cryptoUtil.ParseCertAttribute(subject, "G");
                                report.SignerPosition = cryptoUtil.ParseCertAttribute(subject, "T");
                                report.SignerSurname = cryptoUtil.ParseCertAttribute(subject, "SN");
                                report.SignerName = firstMiddleName.IndexOf(" ") > 0 ? firstMiddleName.Substring(0, firstMiddleName.IndexOf(" ")) : string.Empty;
                                report.SignerPatronymic = firstMiddleName.IndexOf(" ") >= 0 && firstMiddleName.Length > firstMiddleName.IndexOf(" ") + 1 ? firstMiddleName.Substring(firstMiddleName.IndexOf(" ") + 1) : string.Empty;

                                if (string.IsNullOrEmpty(report.SignerPosition))
                                    report.SignerPosition = "Сотрудник с правом подписи";

                                if (SelectedItem.ReceiverInn.Length == 12)
                                {
                                    report.IndividualCreator = new Reporter.Entities.IndividualEntity
                                    {
                                        Inn = SelectedItem.ReceiverInn,
                                        Surname = report.SignerSurname,
                                        Name = report.SignerName,
                                        Patronymic = report.SignerPatronymic
                                    };
                                }

                                string signedFilePath;

                                if (edoSystem.HasZipContent)
                                {
                                    using (ZipArchive zipArchive = ZipFile.Open($"{edoFilesPath}//{SelectedItem.IdDocEdo}//{SelectedItem.FileName}.zip", ZipArchiveMode.Read))
                                    {
                                        var signedEntry = zipArchive.Entries.FirstOrDefault(x => x.Name.StartsWith(SelectedItem.FileName) && x.Name != $"{SelectedItem.FileName}.xml");

                                        if (signedEntry == null)
                                            throw new Exception("Не найден файл подписи продавца.");

                                        if (!File.Exists($"{edoFilesPath}//{SelectedItem.IdDocEdo}//{signedEntry.Name}"))
                                            signedEntry?.ExtractToFile($"{edoFilesPath}//{SelectedItem.IdDocEdo}//{signedEntry.Name}");

                                        signedFilePath = $"{edoFilesPath}//{SelectedItem.IdDocEdo}//{signedEntry.Name}";
                                    }

                                }
                                else
                                {
                                    signedFilePath = $"{edoFilesPath}//{SelectedItem.IdDocEdo}//{SelectedItem.FileName}.xml.sig";

                                    if (!File.Exists(signedFilePath))
                                    {
                                        System.Windows.MessageBox.Show("Не найден файл подписи продавца.", "Ошибка", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                                        return;
                                    }
                                }

                                var sellerSignature = File.ReadAllBytes(signedFilePath);
                                report.Signature = Convert.ToBase64String(sellerSignature);

                                loadContext.SetLoadingText("Сохранение документов");
                                var xmlContent = report.GetXmlContent();
                                var contentBytes = Encoding.GetEncoding(1251).GetBytes(xmlContent);
                                var signature = cryptoUtil.Sign(contentBytes, true);

                                File.WriteAllBytes($"{edoFilesPath}//{SelectedItem.IdDocEdo}//{report.FileName}.xml", contentBytes);
                                File.WriteAllBytes($"{edoFilesPath}//{SelectedItem.IdDocEdo}//{report.FileName}.xml.sig", signature);

                                loadContext.SetLoadingText("Отправка");

                                if (edoSystem.GetType() == typeof(DiadocEdoSystem))
                                    edoSystem.SendRevocationDocument(sellerReport.Function, contentBytes, signature, SelectedItem.IdDocEdo, SelectedItem.ParentEntityId);

                                if (SelectedItem.DocStatus == (int)DocEdoStatus.RevokeRequired)
                                    SelectedItem.DocStatus = (int)DocEdoStatus.Revoked;
                                else
                                    SelectedItem.DocStatus = (int)DocEdoStatus.RevokeRequested;

                                _dataBaseAdapter.Commit(transaction);

                                if(SelectedItem.DocStatus == (int)DocEdoStatus.Revoked)
                                    loadContext.SetSuccessFullLoad("Документ успешно аннулирован.");
                                else
                                    loadContext.SetSuccessFullLoad("Успешно выполнен запрос.");
                            }
                            catch (System.Net.WebException webEx)
                            {
                                transaction.Rollback();
                                _dataBaseAdapter.ReloadEntry(SelectedItem);
                                errorMessage = _log.GetRecursiveInnerException(webEx);
                                titleErrorText = "Произошла ошибка аннулирования документа на удалённом сервере.";
                            }
                            catch (Exception ex)
                            {
                                transaction.Rollback();
                                _dataBaseAdapter.ReloadEntry(SelectedItem);
                                errorMessage = _log.GetRecursiveInnerException(ex);
                                titleErrorText = "Произошла ошибка аннулирования документа.";
                            }
                        }
                    });

                    if (!string.IsNullOrEmpty(errorMessage))
                    {
                        loadWindow.Close();
                        _log.Log(errorMessage);
                        var errorsWindow = new ErrorsWindow(titleErrorText, new List<string>(new string[] { errorMessage }));
                        errorsWindow.ShowDialog();
                    }
                }
            }
        }

        private void SaveDocumentFile()
        {
            if (SelectedItem == null)
            {
                System.Windows.MessageBox.Show(
                    "Не выбран документ для сохранения.", "Ошибка", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                return;
            }

            if (SelectedMyOrganization == null)
            {
                System.Windows.MessageBox.Show(
                    "Не выбрана своя организация для сохранения документов.", "Ошибка", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                return;
            }

            if (SelectedMyOrganization.EdoSystem == null)
            {
                System.Windows.MessageBox.Show(
                    "Регистрация в веб сервисе не была успешной.", "Ошибка", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                return;
            }

            object[] parameters = null;
            string fileName = null;
            var edoSystem = SelectedMyOrganization.EdoSystem;

            try
            {
                if (edoSystem.GetType() == typeof(DiadocEdoSystem))
                {
                    parameters = new object[] { SelectedItem.IdDocEdo, SelectedItem.ParentEntityId };

                    string createDate = SelectedItem.CreateDate?.ToString("dd.MM.yyyy");

                    if (!string.IsNullOrEmpty(createDate))
                        fileName = $"УПД № {SelectedItem.Name} от {SelectedItem.CreateDate?.ToString("dd.MM.yyyy")}";
                    else
                        fileName = $"УПД № {SelectedItem.Name}";
                }
                else if (edoSystem.GetType() == typeof(EdoLiteSystem))
                {
                    parameters = new object[] { SelectedItem.IdDocEdo };
                    fileName = SelectedItem.Name;
                }

                if (parameters == null)
                    return;

                var contentBytes = edoSystem.GetDocumentPrintForm(parameters);

                var changePathDialog = new Microsoft.Win32.SaveFileDialog();
                changePathDialog.Title = "Сохранение файла";
                changePathDialog.Filter = "PDF Files|*.pdf";
                changePathDialog.FileName = fileName;

                if(changePathDialog.ShowDialog() == true)
                {
                    File.WriteAllBytes(changePathDialog.FileName, contentBytes);

                    var loadWindow = new LoadWindow();
                    loadWindow.GetLoadContext().SetSuccessFullLoad("Файл успешно сохранён.");
                    loadWindow.ShowDialog();
                }
            }
            catch (System.Net.WebException webEx)
            {
                string errorMessage = _log.GetRecursiveInnerException(webEx);
                _log.Log(errorMessage);

                var errorsWindow = new ErrorsWindow("Произошла ошибка сохранения документа в файл на удалённом сервере.", new List<string>(new string[] { errorMessage }));
                errorsWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                string errorMessage = _log.GetRecursiveInnerException(ex);
                _log.Log(errorMessage);

                var errorsWindow = new ErrorsWindow("Произошла ошибка сохранения документа в файл.", new List<string>(new string[] { errorMessage }));
                errorsWindow.ShowDialog();
            }
        }

        private bool MarkedCodesOwnerCheck(IEnumerable<string> markedCodes, string ownerInn, ErrorTextModel errorModel = null)
        {
            var positionInArray = 0;
            var honestMarkSystem = SelectedMyOrganization.HonestMarkSystem;

            var errorCodes = new List<MarkCodeInfo>();
            while (positionInArray < markedCodes.Count())
            {
                int length = markedCodes.Count() - positionInArray > 500 ? 500 : markedCodes.Count() - positionInArray;
                var markedCodesInfo = honestMarkSystem.GetMarkedCodesInfo(ProductGroupsEnum.None, markedCodes.Skip(positionInArray).Take(length).ToArray());

                if (markedCodesInfo.Any(m => m?.CisInfo?.OwnerInn != ownerInn))
                {
                    if (errorModel == null)
                        return false;
                    else
                        errorCodes.AddRange(markedCodesInfo.Where(m => m?.CisInfo?.OwnerInn != ownerInn));
                }

                positionInArray += 500;
            }

            if (errorCodes.Count > 0)
            {
                if(errorModel != null)
                    errorModel.ErrorMessage += string.Join("\n", errorCodes.Where(s => s.CisInfo != null).Select(s => s.CisInfo.Cis)) + "\n";

                return false;
            }

            return true;
        }

        private void SaveMarkedCodesToDataBase(byte[] sellerFileContent, decimal? idDoc = null, decimal? oldIdDoc = null, List<decimal> idGoods = null)
        {
            if (idGoods != null)
                if (idGoods.Count == 0)
                    return;

            ErrorTextModel errorModel = null;
            var reporterDll = new Reporter.ReporterDll();
            var report = reporterDll.ParseDocument<Reporter.Reports.UniversalTransferSellerDocument>(sellerFileContent);
            var honestMarkSystem = SelectedMyOrganization.HonestMarkSystem;
            string errorMessage = null;

            if (report.Products == null || report.Products.Count == 0)
                return;

            var markedCodes = new List<KeyValuePair<string, string>>();

            var productsWithTransportCodes = report?.Products?.Where(p => p.TransportPackingIdentificationCode != null && p.TransportPackingIdentificationCode.Count > 0);

            var transportCodes = productsWithTransportCodes?.SelectMany(p => p.TransportPackingIdentificationCode)?.Distinct() ?? new List<string>();

            if (transportCodes.Count() > 0)
                markedCodes = honestMarkSystem.GetCodesByThePiece(transportCodes, markedCodes);

            foreach (var product in report.Products)
                if (product.MarkedCodes != null && product.MarkedCodes.Count > 0)
                    markedCodes = honestMarkSystem.GetCodesByThePiece(product.MarkedCodes, markedCodes);

            if (markedCodes.Count == 0)
                return;
            else
            {
                var markedCodesInBase = _dataBaseAdapter.GetMarkedCodesByDocumentId(idDoc) ?? new List<string>();

                if (markedCodesInBase.Count() > markedCodes.Count)
                    throw new Exception("Число сохранённых кодов в базе по документу больше числа кодов в документе.");
            }

            var markedCodesArray = markedCodes.Select(m => m.Key);

            errorModel = new ErrorTextModel("В списке кодов маркировки есть не принадлежащие отправителю:\n");
            if (!MarkedCodesOwnerCheck(markedCodesArray, SelectedItem.SenderInn, errorModel))
                throw new Exception(errorModel.ErrorMessage);
            else
                errorModel = null;

            var barCodes = markedCodes.Select(m => m.Value).Distinct();
            var refBarCodes = _dataBaseAdapter.GetRefBarCodesByBarCodes(barCodes);

            if (idGoods != null)
                refBarCodes = refBarCodes.Where(r => idGoods.Exists(g => g == (r as RefBarCode)?.IdGood));

            var markedCodesByRefBarCodes = from markedCode in markedCodes
                                           let refBarCode = refBarCodes.FirstOrDefault(r => (r as RefBarCode)?.BarCode == markedCode.Value)
                                           select new { Item=markedCode.Key, BarCode = markedCode.Value, IdGood = (refBarCode as RefBarCode)?.IdGood };

            if (markedCodesByRefBarCodes.Any(m => m.IdGood == null))
                throw new Exception("Среди штрихкодов маркированных товаров есть несопоставленные с ID товары, либо штрихкоды отсутствуют в карточках товаров:\r\n"
                    +string.Join(", \r\n", markedCodesByRefBarCodes.Where(m => m.IdGood == null).Select(m => m.BarCode).Distinct()));

            var productsByRefBarCodes = from product in report.Products
                                        join refBarCode in refBarCodes
                                        on product.BarCode equals (refBarCode as RefBarCode).BarCode
                                        select new { Product=product, (refBarCode as RefBarCode).BarCode, (refBarCode as RefBarCode).IdGood };

            var productGroups = from markedCodeByRefBarCodes in markedCodesByRefBarCodes
                                group markedCodeByRefBarCodes by markedCodeByRefBarCodes.IdGood into gr
                                join productByRefBarCodes in productsByRefBarCodes on gr.Key equals productByRefBarCodes.IdGood
                                select new
                                {
                                    productByRefBarCodes.Product.Quantity,
                                    productByRefBarCodes.Product.BarCode,
                                    productByRefBarCodes.Product.Description,
                                    productByRefBarCodes.Product.Number,
                                    Count = gr.Count(),
                                    Items = gr.Select(g => g.Item).ToList()
                                };

            var productGroupsNotEquals = productGroups.Where(g => g.Count != g.Quantity).ToList();

            foreach (var productGroup in productGroupsNotEquals)
            {
                errorMessage = errorMessage == null ? $"Количество товара с наименованием: \n{productGroup.Description}\nНе равно количеству кодов маркировки.\n" +
                    $"Количество кодов маркировки-{productGroup.Count}, товара-{productGroup.Quantity}."
                    : $"{errorMessage}\n\nКоличество товара с наименованием: \n{productGroup.Description}\nНе равно количеству кодов маркировки.\n" +
                    $"Количество кодов маркировки-{productGroup.Count}, товара-{productGroup.Quantity}.";
            }

            if (!string.IsNullOrEmpty(errorMessage))
                if (System.Windows.MessageBox.Show(errorMessage +
                        $"\nВсё равно хотите отправить документ?", "Внимание",
                        System.Windows.MessageBoxButton.YesNo,
                        System.Windows.MessageBoxImage.Question) != System.Windows.MessageBoxResult.Yes)
                    throw new Exception("Попытка отправки прервана пользователем." + errorMessage);

            if (idDoc == null)
                idDoc = SelectedItem.IdDocJournal.Value;

            productGroups = productGroups.ToList();

            var markedCodesByGoods = new List<KeyValuePair<decimal, List<string>>>();
            foreach (var productGroup in productGroups)
            {
                var detail = Details?.FirstOrDefault(d => d.DetailNumber == productGroup.Number);

                if (detail == null)
                    throw new Exception($"Не найден товар с названием {productGroup.Description}.");

                if (detail.IdGood == null)
                    throw new Exception($"Товар {productGroup.Description} не сопоставлен.");

                markedCodesByGoods.Add(new KeyValuePair<decimal, List<string>>(detail.IdGood.Value, productGroup.Items));
            }

            //IEnumerable<string> updatedCodes = null;
            //if(oldIdDoc != null)
            //    updatedCodes = _dataBaseAdapter.UpdateCodes(idDoc.Value, markedCodesByGoods.SelectMany(s => s.Value), oldIdDoc);

            _dataBaseAdapter.AddMarkedCodes(idDoc.Value, markedCodesByGoods);//, updatedCodes);
        }

        private void UpdateProperties()
        {
            OnPropertyChanged("ItemsList");
            OnPropertyChanged("SelectedItem");
            OnPropertyChanged("SelectedMyOrganization");
        }

        public void SaveParameters(ConsignorOrganization myOrganization, params object[] parameters)
        {
            var edoSystem = myOrganization.EdoSystem;
            edoSystem?.SaveParameters(parameters);
        }

        public List<IEdoSystemDocument<string>> GetNewDocuments(ConsignorOrganization myOrganization, out object[] parameters)
        {
            if (myOrganization.EdoSystem == null)
            {
                parameters = null;
                return null;
            }

            var edoSystem = myOrganization.EdoSystem;
            if (edoSystem.GetType() == typeof(EdoLiteSystem))
            {
                var docCount = edoSystem.GetDocumentsCount();
                var edoDocCount = ConfigSet.Configs.Config.GetInstance()?.EdoDocCount ?? 0;

                parameters = new object[2] { docCount, DateTime.Now };
                if (edoDocCount < docCount)
                {
                    var documentList = edoSystem.GetDocuments(DocumentInOutType.Inbox, docCount - edoDocCount,
                        ConfigSet.Configs.Config.GetInstance().EdoLastDocDateTime);

                    return documentList ?? new List<IEdoSystemDocument<string>>();
                }
                else
                    return new List<IEdoSystemDocument<string>>();
            }
            else if(edoSystem.GetType() == typeof(DiadocEdoSystem))
            {
                parameters = new object[1] { DateTime.Now };

                var newDocuments = edoSystem.GetDocuments(DocumentInOutType.Inbox);
                return newDocuments;
            }
            else
            {
                parameters = null;
                return null;
            }
        }

        public bool SaveNewDocument(ConsignorOrganization myOrganization, IEdoSystemDocument<string> document, out byte[] fileBytes)
        {
            if(!Directory.Exists(edoFilesPath))
                Directory.CreateDirectory(edoFilesPath);

            string dirPath = $"{edoFilesPath}//{document.EdoId}";
            Directory.CreateDirectory(dirPath);

            byte[] signature = null;

            var edoSystem = myOrganization.EdoSystem;
            if (edoSystem.HasZipContent)
                fileBytes = edoSystem.GetDocumentContent(document, DocumentInOutType.Inbox);
            else
                fileBytes = edoSystem.GetDocumentContent(document, out signature, DocumentInOutType.Inbox);

            byte[] zipBytes;
            try
            {
                zipBytes = edoSystem.GetZipContent(document.EdoId, DocumentInOutType.Inbox);
            }
            catch
            {
                zipBytes = null;
            }

            if (edoSystem.GetType() == typeof(EdoLiteSystem))
            {
                if (document.DocType == (int)EdoLiteDocumentType.DpUkdDis || document.DocType == (int)EdoLiteDocumentType.DpUkdDisInfoBuyer
                    || document.DocType == (int)EdoLiteDocumentType.DpUkdInvoice || document.DocType == (int)EdoLiteDocumentType.DpUkdInvoiceDis
                    || document.DocType == (int)EdoLiteDocumentType.DpUkdInvoiceDisInfoBuyer || document.DocType == (int)EdoLiteDocumentType.DpUpdDop
                    || document.DocType == (int)EdoLiteDocumentType.DpUpdDopInfoBuyer || document.DocType == (int)EdoLiteDocumentType.DpUpdInvoiceDop
                    || document.DocType == (int)EdoLiteDocumentType.DpUpdInvoiceDopInfoBuyer || document.DocType == (int)EdoLiteDocumentType.DpUpdiDop
                    || document.DocType == (int)EdoLiteDocumentType.DpUpdiDopInfoBuyer || document.DocType == (int)EdoLiteDocumentType.DpUpdiInvoiceDop
                    || document.DocType == (int)EdoLiteDocumentType.DpUpdiInvoiceDopInfoBuyer)
                {
                    var xml = Encoding.GetEncoding(1251).GetString(fileBytes);
                    var xmlDocument = new XmlDocument();
                    xmlDocument.LoadXml(xml);
                    string docName = xmlDocument.LastChild.Attributes["ИдФайл"].Value;
                    string filePath = $"{dirPath}//{docName}.xml";
                    xmlDocument.Save(filePath);

                    if (zipBytes != null)
                        File.WriteAllBytes($"{dirPath}//{docName}.zip", zipBytes);

                    return true;
                }
                else
                    return false;
            }
            else if (edoSystem.GetType() == typeof(DiadocEdoSystem))
            {
                var doc = document as DiadocEdoDocument;

                if (string.IsNullOrEmpty(doc?.FileName))
                    return false;

                if (doc.DocumentType != Diadoc.Api.Proto.DocumentType.XmlAcceptanceCertificate && doc.DocumentType != Diadoc.Api.Proto.DocumentType.Invoice && 
                    doc.DocumentType != Diadoc.Api.Proto.DocumentType.XmlTorg12 && doc.DocumentType != Diadoc.Api.Proto.DocumentType.UniversalTransferDocument
                    && doc.DocumentType != Diadoc.Api.Proto.DocumentType.UniversalTransferDocumentRevision)
                    return false;

                var xml = Encoding.GetEncoding(1251).GetString(fileBytes);
                var xmlDocument = new XmlDocument();
                xmlDocument.LoadXml(xml);

                var fileNameLength = doc.FileName.LastIndexOf('.');

                if(fileNameLength < 0)
                    fileNameLength = doc.FileName.Length;

                var fileName = doc.FileName.Substring(0, fileNameLength);

                string filePath = $"{dirPath}//{fileName}.xml";
                string signatureFilePath = $"{dirPath}//{fileName}.xml.sig";
                xmlDocument.Save(filePath);
                File.WriteAllBytes(signatureFilePath, signature);

                return true;
            }
            else
                return false;
        }

        public void SaveOrgData(ConsignorOrganization myOrganization)
        {
            if(_dataBaseAdapter.GetType() == typeof(DiadocEdoToDataBase))
            {
                var edoSystem = myOrganization.EdoSystem as DiadocEdoSystem;

                if (edoSystem == null)
                    return;

                var counteragentsBoxIdsForOrganization = edoSystem.GetCounteragentsBoxesForOrganization(myOrganization.OrgInn);
                ((DiadocEdoToDataBase)_dataBaseAdapter).SetPermittedBoxIds(counteragentsBoxIdsForOrganization);

                myOrganization.OrgKpp = edoSystem.GetKppForMyOrganization(myOrganization.OrgInn);
            }
        }

        public void UpdateIdGood()
        {
            if(SelectedDetail == null)
            {
                System.Windows.MessageBox.Show(
                    "Не выбран товар для сопоставления.", "Ошибка", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                return;
            }

            try
            {
                var refGoods = _dataBaseAdapter.GetRefGoodsByBarCode(SelectedDetail.BarCode);

                if (refGoods.Count == 0)
                    refGoods = _dataBaseAdapter.GetAllRefGoods();

                var refGoodsModel = new RefGoodsModel(refGoods.Cast<RefGood>());
                var refGoodsWindow = new RefGoodsWindow();
                refGoodsWindow.DataContext = refGoodsModel;

                if (refGoodsWindow.ShowDialog() == true)
                {
                    var oldIdGood = SelectedDetail?.IdGood;
                    SelectedDetail.IdGood = refGoodsModel.SelectedItem.Id;
                    OnPropertyChanged("SelectedDetail");

                    if(SelectedItem?.IdDocJournal != null && SelectedItem?.IdDocJournal != 0 &&
                        oldIdGood != null && oldIdGood != 0)
                    {
                        _dataBaseAdapter.UpdateRefGoodForMarkedCodes(SelectedItem.IdDocJournal.Value, oldIdGood.Value, SelectedDetail.IdGood.Value);
                        _dataBaseAdapter.Commit();
                    }
                }
            }
            catch(Exception ex)
            {
                var errorMessage = $"Exception: {_log.GetRecursiveInnerException(ex)}";
                var errorsWindow = new ErrorsWindow("Произошла ошибка привязки Id для товара.", new List<string>(new string[] { errorMessage }));
                _log.Log(errorMessage);
                errorsWindow.ShowDialog();
            }
        }

        public void ChangeMyOrganization()
        {
            if (SelectedMyOrganization == null)
            {
                ItemsList = new System.Collections.ObjectModel.ObservableCollection<DocEdoPurchasing>();
                SelectedItem = null;
                UpdateProperties();
                return;
            }

            var docs = _dataBaseAdapter.GetAllDocuments(DateFrom, DateTo).Cast<DocEdoPurchasing>()
                .Where(d => d.ReceiverInn == SelectedMyOrganization.OrgInn && SelectedMyOrganization.ShipperOrgInns.Exists(s => s == d.SenderInn));

            var result = SelectedMyOrganization.EdoSystem?.Authorization() ?? false;

            if (!result)
                SelectedMyOrganization.EdoSystem = null;

            result = result && (SelectedMyOrganization.HonestMarkSystem?.Authorization() ?? false);

            if (SelectedMyOrganization.EdoSystem != null && !result)
                SelectedMyOrganization.HonestMarkSystem = null;

            if (result)
                ItemsList = new System.Collections.ObjectModel.ObservableCollection<DocEdoPurchasing>(docs);
            else
                ItemsList = new System.Collections.ObjectModel.ObservableCollection<DocEdoPurchasing>();

            SelectedItem = null;
            UpdateProperties();
        }

        protected override void OnDispose()
        {
            _dataBaseAdapter.Dispose();
        }
    }
}
