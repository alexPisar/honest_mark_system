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

        private IEdoSystem _edoSystem;
        private Interfaces.IEdoDataBaseAdapter<AbtDbContext> _dataBaseAdapter;
        private UtilitesLibrary.Service.CryptoUtil _cryptoUtil;
        private WebSystems.Systems.HonestMarkSystem _honestMarkSystem;

        public string EdoProgramVersion => $"{_applicationName} {currentVersion}";

        public List<DocEdoPurchasingDetail> Details => SelectedItem?.Details;
        public DocEdoPurchasingDetail SelectedDetail { get; set; }

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
        public RelayCommand WithdrawalCodesCommand => new RelayCommand((o) => { WithdrawalCodes(); });
        public RelayCommand RejectDocumentCommand => new RelayCommand((o) => { RejectDocument(); });
        public RelayCommand RevokeDocumentCommand => new RelayCommand((o) => { RevokeDocument(); });
        public RelayCommand SaveDocumentFileCommand => new RelayCommand((o) => { SaveDocumentFile(); });

        public MainViewModel(IEdoSystem edoSystem,
            WebSystems.Systems.HonestMarkSystem honestMarkSystem, 
            Interfaces.IEdoDataBaseAdapter<AbtDbContext> dataBaseAdapter, 
            UtilitesLibrary.Service.CryptoUtil cryptoUtil)
        {
            _edoSystem = edoSystem;
            _honestMarkSystem = honestMarkSystem;
            _cryptoUtil = cryptoUtil;
            _dataBaseAdapter = dataBaseAdapter;

            ItemsList = new System.Collections.ObjectModel.ObservableCollection<DocEdoPurchasing>();
        }

        public System.Windows.Window Owner { get; set; }

        private void Refresh()
        {
            _dataBaseAdapter.Dispose();
            _dataBaseAdapter.InitializeContext();
            object[] parameters = null;

            List<IEdoSystemDocument<string>> documents = new List<IEdoSystemDocument<string>>();
            try
            {
                documents = GetNewDocuments(out parameters);
            }
            catch(System.Net.WebException webEx)
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
                try
                {
                    foreach (var doc in documents)
                    {
                        if (!_dataBaseAdapter.DocumentCanBeAddedByUser(doc))
                            continue;

                        byte[] docContentBytes;

                        if(!SaveNewDocument(doc, out docContentBytes))
                            continue;

                        var docFromDb = (DocEdoPurchasing)_dataBaseAdapter.GetDocumentFromDb(doc);

                        if (docFromDb == null)
                        {
                            _edoSystem.SendReceivingConfirmationEventHandler?.Invoke(this, new WebSystems.EventArgs.SendReceivingConfirmationEventArgs { Document = doc});
                            _dataBaseAdapter.AddDocumentToDataBase(doc, docContentBytes, DocumentInOutType.Inbox);
                        }
                    }
                    _dataBaseAdapter.Commit();
                    SaveParameters(parameters);
                }
                catch(Exception ex)
                {
                    _dataBaseAdapter.Rollback();
                    string errorMessage = _log.GetRecursiveInnerException(ex);
                    _log.Log(errorMessage);

                    var errorsWindow = new ErrorsWindow("Произошла ошибка.", new List<string>(new string[] { errorMessage }));
                    errorsWindow.ShowDialog();
                }
            }

            var docs = _dataBaseAdapter.GetAllDocuments(DateFrom, DateTo).Cast<DocEdoPurchasing>();
            ItemsList = new System.Collections.ObjectModel.ObservableCollection<DocEdoPurchasing>(docs);
            SelectedItem = null;

            try
            {
                var processingDocuments = docs.Where(d => d.DocStatus == (int)DocEdoStatus.Sent).ToList();

                foreach (var processingDocument in processingDocuments)
                {
                    if (_honestMarkSystem != null)
                    {
                        var docProcessingInfo = _honestMarkSystem.GetEdoDocumentProcessInfo(processingDocument.FileName);

                        if (docProcessingInfo.Code == EdoLiteProcessResultStatus.SUCCESS)
                        {
                            processingDocument.DocStatus = (int)DocEdoStatus.Processed;
                            LoadStatus(processingDocument);

                            if (processingDocument.IdDocJournal != null)
                                _dataBaseAdapter.UpdateMarkedCodeIncomingStatuses(processingDocument.IdDocJournal.Value, MarkedCodeComingStatus.Accepted);
                        }
                        else if (docProcessingInfo.Code == EdoLiteProcessResultStatus.FAILED)
                        {
                            processingDocument.DocStatus = (int)DocEdoStatus.ProcessingError;

                            var failedOperations = docProcessingInfo?.Operations?.Select(o => o.Details)?.Where(o => o.Successful == false);

                            var errors = failedOperations.SelectMany(f => f.Errors);

                            var errorsList = new List<string>();
                            foreach(var error in errors)
                            {
                                if (!string.IsNullOrEmpty(error.Text))
                                    errorsList.Add($"Произошла ошибка с кодом:{error.Code} \nОписание:{error.Text}\n");
                                else if (!string.IsNullOrEmpty(error?.Error?.Detail))
                                    errorsList.Add($"Произошла ошибка с кодом:{error.Code} \nДетали:{error?.Error?.Detail}\n");
                                else
                                    errorsList.Add($"Произошла ошибка с кодом:{error.Code}\n");
                            }
                            processingDocument.ErrorMessage = string.Join("\n\n", errorsList);
                            LoadStatus(processingDocument);
                        }
                    }
                }

                if(processingDocuments.Exists(p => p.DocStatus != (int)DocEdoStatus.Sent))
                    _dataBaseAdapter.Commit();
            }
            catch (Exception ex)
            {
                _dataBaseAdapter.Rollback();
                string errorMessage = _log.GetRecursiveInnerException(ex);
                _log.Log(errorMessage);

                //var errorsWindow = new ErrorsWindow("Произошла ошибка обновления статусов.", new List<string>(new string[] { errorMessage }));
                //errorsWindow.ShowDialog();
            }

            try
            {
                var newDocuments = docs.Where(d => d.DocStatus == (int)DocEdoStatus.New || d.DocStatus == (int)DocEdoStatus.RevokeRequested).ToList();

                foreach (var newDocument in newDocuments)
                {
                    if (_edoSystem.GetType() == typeof(DiadocEdoSystem))
                        newDocument.DocStatus = (int)_edoSystem.GetCurrentStatus(newDocument.DocStatus, newDocument.IdDocEdo, newDocument.ParentEntityId);
                }

                if(newDocuments.Exists(d => d.DocStatus != (int)DocEdoStatus.New && d.DocStatus != (int)DocEdoStatus.RevokeRequested))
                    _dataBaseAdapter.Commit();
            }
            catch(Exception ex)
            {
                _dataBaseAdapter.Rollback();
                string errorMessage = _log.GetRecursiveInnerException(ex);
                _log.Log(errorMessage);

                var errorsWindow = new ErrorsWindow("Произошла ошибка обновления статусов.", new List<string>(new string[] { errorMessage }));
                errorsWindow.ShowDialog();
            }

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
                _dataBaseAdapter.Rollback();
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

            try
            {
                SelectedItem.IdDocJournal = null;
                OnPropertyChanged("SelectedItem");
                _dataBaseAdapter.Commit();

                LoadWindow loadWindow = new LoadWindow();
                loadWindow.GetLoadContext().SetSuccessFullLoad("Документ отвязан успешно.");
                loadWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                _dataBaseAdapter.Rollback();
                var errorMessage = $"Exception: {_log.GetRecursiveInnerException(ex)}";
                var errorsWindow = new ErrorsWindow("Произошла ошибка отвязки документа.", new List<string>(new string[] { errorMessage }));
                _log.Log(errorMessage);
                errorsWindow.ShowDialog();
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

            if (Details.Exists(d => d?.IdGood == null))
            {
                System.Windows.MessageBox.Show(
                    "В списке товаров есть несопоставленные с ID товары.", "", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                return;
            }

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
                    try
                    {
                        decimal? oldIdDoc = SelectedItem?.IdDocJournal;
                        SelectedItem.IdDocJournal = docPurchasingModel.SelectedItem.Id;

                        if (_honestMarkSystem != null)
                        {
                            loadContext.SetLoadingText("Сохранение кодов");
                            var sellerXmlDocument = new XmlDocument();
                            sellerXmlDocument.Load($"{edoFilesPath}//{SelectedItem.IdDocEdo}//{SelectedItem.FileName}.xml");
                            var docSellerContent = Encoding.GetEncoding(1251).GetBytes(sellerXmlDocument.OuterXml);
                            SaveMarkedCodesToDataBase(docSellerContent, SelectedItem.IdDocJournal, oldIdDoc);
                        }

                        _dataBaseAdapter.Commit();
                        OnPropertyChanged("SelectedItem");
                        loadContext.SetSuccessFullLoad("Документ был успешно сопоставлен.");
                    }
                    catch (Exception ex)
                    {
                        _dataBaseAdapter.Rollback();
                        errorMessage = _log.GetRecursiveInnerException(ex);
                        _log.Log(errorMessage);
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

            if (SelectedItem.IdDocJournal == null)
            {
                System.Windows.MessageBox.Show(
                    "Данный документ не сопоставлен с документом из трейдера.", "Ошибка", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                return;
            }

            if (_edoSystem.HasZipContent && !File.Exists($"{edoFilesPath}//{SelectedItem.IdDocEdo}//{SelectedItem.FileName}.zip"))
            {
                System.Windows.MessageBox.Show(
                    "Не найден zip файл документооборота.", "Ошибка", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                return;
            }

            if (Details.Exists(d => d?.IdGood == null))
            {
                System.Windows.MessageBox.Show(
                    "В списке товаров есть несопоставленные с ID товары.", "", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                return;
            }

            if (Details.GroupBy(d => d.IdGood).Any(g => g.Count() > 1))
            {
                System.Windows.MessageBox.Show(
                    "В списке товаров есть одинаковые ID товаров.", "", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                return;
            }

            if (!File.Exists($"{edoFilesPath}//{SelectedItem.IdDocEdo}//{SelectedItem.FileName}.xml"))
            {
                if (_edoSystem.HasZipContent)
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

            var signWindow = new BuyerSignWindow(_cryptoUtil, $"{edoFilesPath}//{SelectedItem.IdDocEdo}//{SelectedItem.FileName}.xml");
            signWindow.SetDefaultParameters(_edoSystem.GetCertSubject(), SelectedItem);
            signWindow.Report.EdoProgramVersion = this.EdoProgramVersion;

            string signedFilePath;

            if (_edoSystem.HasZipContent)
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

                loadWindow.Show();

                await Task.Run(() => 
                {
                    try
                    {
                        var markedCodesArray = _dataBaseAdapter.GetMarkedCodesByDocumentId(SelectedItem.IdDocJournal.Value);

                        if (_honestMarkSystem != null && markedCodesArray != null && !MarkedCodesOwnerCheck(markedCodesArray, SelectedItem.SenderInn))
                            throw new Exception("В списке кодов маркировки есть не принадлежащие отправителю.");

                        if (_dataBaseAdapter.IsExistsNotReceivedCodes(SelectedItem.IdDocJournal.Value))
                            throw new Exception("В списке кодов есть непропиканные, либо оприходованные коды");

                        var xml = reportForSend.GetXmlContent();
                        var fileBytes = Encoding.GetEncoding(1251).GetBytes(xml);
                        var signature = _cryptoUtil.Sign(fileBytes, true);

                        File.WriteAllBytes($"{edoFilesPath}//{SelectedItem.IdDocEdo}//{reportForSend.FileName}.xml", fileBytes);
                        File.WriteAllBytes($"{edoFilesPath}//{SelectedItem.IdDocEdo}//{reportForSend.FileName}.xml.sig", signature);

                        var directory = new DirectoryInfo(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location));

                        loadContext.SetLoadingText("Подписание и отправка");
                        if (_edoSystem.GetType() == typeof(EdoLiteSystem))
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
                            _edoSystem.SendDocument(SelectedItem.IdDocEdo, fileBytes, signature, content);
                        }
                        else if(_edoSystem.GetType() == typeof(DiadocEdoSystem))
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

                            _edoSystem.SendDocument(SelectedItem.IdDocEdo, fileBytes, signature, SelectedItem.ParentEntityId, SelectedItem.IdDocType);
                        }

                        SelectedItem.SignatureFileName = reportForSend.FileName;

                        if (reportForSend.FileName.StartsWith("ON_NSCHFDOPPOKMARK"))
                            SelectedItem.DocStatus = (int)DocEdoStatus.Sent;
                        else
                            SelectedItem.DocStatus = (int)DocEdoStatus.Processed;

                        LoadStatus(SelectedItem);
                        UpdateProperties();

                        _dataBaseAdapter.Commit();
                        loadContext.SetSuccessFullLoad("Данные были успешно загружены.");
                    }
                    catch (System.Net.WebException webEx)
                    {
                        _dataBaseAdapter.Rollback();
                        errorMessage = _log.GetRecursiveInnerException(webEx);
                        titleErrorText = "Произошла ошибка отправки на удалённом сервере.";
                    }
                    catch (Exception ex)
                    {
                        _dataBaseAdapter.Rollback();
                        errorMessage = _log.GetRecursiveInnerException(ex);
                        titleErrorText = "Произошла ошибка отправки.";
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

        private async void ExportToTrader()
        {
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

            if(_honestMarkSystem == null)
            {
                if(System.Windows.MessageBox.Show(
                    "Авторизация в Честном знаке не была успешной. \nНевозможно экспортировать коды маркировки.\nВы точно хотите экспортировать документ?", "Ошибка авторизации", System.Windows.MessageBoxButton.YesNo, System.Windows.MessageBoxImage.Question) != System.Windows.MessageBoxResult.Yes)
                    return;
            }

            string errorMessage = null;
            if (!File.Exists($"{edoFilesPath}//{SelectedItem.IdDocEdo}//{SelectedItem.FileName}.xml"))
            {
                if (_edoSystem.HasZipContent)
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
                try
                {
                    var sellerXmlDocument = new XmlDocument();
                    sellerXmlDocument.Load($"{edoFilesPath}//{SelectedItem.IdDocEdo}//{SelectedItem.FileName}.xml");
                    var docSellerContent = Encoding.GetEncoding(1251).GetBytes(sellerXmlDocument.OuterXml);

                    loadContext.SetLoadingText("Сохранение документа");
                    var docJournalId = _dataBaseAdapter.ExportDocument(SelectedItem);

                    if (_honestMarkSystem != null)
                    {
                        loadContext.SetLoadingText("Сохранение кодов маркировки");
                        SaveMarkedCodesToDataBase(docSellerContent, docJournalId);
                    }

                    _dataBaseAdapter.Commit();
                    loadContext.SetSuccessFullLoad("Экспорт выполнен успешно.");
                }
                catch (Exception ex)
                {
                    _dataBaseAdapter.Rollback();
                    errorMessage = _log.GetRecursiveInnerException(ex);
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

        private void WithdrawalCodes()
        {
            if(_honestMarkSystem == null)
            {
                System.Windows.MessageBox.Show(
                    "Невозможно оформить вывод из оборота.\nНе пройдена авторизация в Честном знаке.", "Ошибка", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                return;
            }

            var markedCodes = _dataBaseAdapter.GetAllMarkedCodes().Cast<DocGoodsDetailsLabels>().ToList();

            var withdrawalWindow = new ChangeMarkedCodesWindow();
            withdrawalWindow.DataContext = new ChangeMarkedCodesModel(markedCodes, _honestMarkSystem);
            withdrawalWindow.ShowDialog();
        }

        private async void RejectDocument()
        {
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

                loadWindow.Show();
                await Task.Run(() =>
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

                        var subject = _edoSystem.GetCertSubject();
                        var firstMiddleName = _cryptoUtil.ParseCertAttribute(subject, "G");
                        report.SignerPosition = _cryptoUtil.ParseCertAttribute(subject, "T");
                        report.SignerSurname = _cryptoUtil.ParseCertAttribute(subject, "SN");
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

                        if (_edoSystem.HasZipContent)
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
                        var signature = _cryptoUtil.Sign(contentBytes, true);

                        File.WriteAllBytes($"{edoFilesPath}//{SelectedItem.IdDocEdo}//{report.FileName}.xml", contentBytes);
                        File.WriteAllBytes($"{edoFilesPath}//{SelectedItem.IdDocEdo}//{report.FileName}.xml.sig", signature);

                        loadContext.SetLoadingText("Отправка");

                        if(_edoSystem.GetType() == typeof(DiadocEdoSystem))
                            _edoSystem.SendRejectionDocument(sellerReport.Function, contentBytes, signature, SelectedItem.IdDocEdo, SelectedItem.ParentEntityId);

                        SelectedItem.DocStatus = (int)DocEdoStatus.Rejected;

                        _dataBaseAdapter.Commit();
                        loadContext.SetSuccessFullLoad("Документ успешно отклонён.");
                    }
                    catch (System.Net.WebException webEx)
                    {
                        _dataBaseAdapter.Rollback();
                        errorMessage = _log.GetRecursiveInnerException(webEx);
                        titleErrorText = "Произошла ошибка отклонения документа на удалённом сервере.";
                    }
                    catch (Exception ex)
                    {
                        _dataBaseAdapter.Rollback();
                        errorMessage = _log.GetRecursiveInnerException(ex);
                        titleErrorText = "Произошла ошибка отклонения документа.";
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

                    if (_edoSystem.GetType() == typeof(DiadocEdoSystem))
                    {
                        loadWindow.Show();
                        byte[] sellerSignature;
                        var revokeDocumentEntity = (Diadoc.Api.Proto.Events.Entity)_edoSystem.GetRevokeDocument(out fileName, out sellerSignature, SelectedItem.CounteragentEdoBoxId, SelectedItem.IdDocEdo, SelectedItem.ParentEntityId);
                        var revokeDocumentContent = revokeDocumentEntity?.Content?.Data;

                        if (revokeDocumentContent == null)
                            throw new Exception("Не удалось загрузить документ Запрос на аннулирование.");

                        if (comfirmRevokeWindow.Result == RevokeRequestDialogResult.Revoke)
                        {
                            var signature = _cryptoUtil.Sign(revokeDocumentContent, true);

                            File.WriteAllBytes($"{edoFilesPath}//{SelectedItem.IdDocEdo}//{fileName}", revokeDocumentContent);
                            File.WriteAllBytes($"{edoFilesPath}//{SelectedItem.IdDocEdo}//{fileName}.sig", signature);

                            _edoSystem.SendRevokeConfirmation(signature, SelectedItem.IdDocEdo, revokeDocumentEntity.EntityId);

                            SelectedItem.DocStatus = (int)DocEdoStatus.Revoked;
                            _dataBaseAdapter.Commit();
                        }
                        else if(comfirmRevokeWindow.Result == RevokeRequestDialogResult.RejectRevoke)
                        {
                            var report = comfirmRevokeWindow.Report;
                            var loadContext = loadWindow.GetLoadContext();

                            await Task.Run(() => 
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

                                    var subject = _edoSystem.GetCertSubject();
                                    var firstMiddleName = _cryptoUtil.ParseCertAttribute(subject, "G");
                                    report.SignerPosition = _cryptoUtil.ParseCertAttribute(subject, "T");
                                    report.SignerSurname = _cryptoUtil.ParseCertAttribute(subject, "SN");
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
                                    var signature = _cryptoUtil.Sign(contentBytes, true);

                                    File.WriteAllBytes($"{edoFilesPath}//{SelectedItem.IdDocEdo}//{report.FileName}.xml", contentBytes);
                                    File.WriteAllBytes($"{edoFilesPath}//{SelectedItem.IdDocEdo}//{report.FileName}.xml.sig", signature);

                                    loadContext.SetLoadingText("Отправка");

                                    _edoSystem.SendRejectionDocument(sellerReport.Function, contentBytes, signature, SelectedItem.IdDocEdo, revokeDocumentEntity.EntityId);

                                    SelectedItem.DocStatus = (int)DocEdoStatus.RejectRevoke;
                                    _dataBaseAdapter.Commit();
                                }
                                catch(Exception ex)
                                {
                                    exception = ex;
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
                    _dataBaseAdapter.Rollback();
                    string errMessage = _log.GetRecursiveInnerException(webEx);
                    _log.Log(errMessage);

                    var errorsWindow = new ErrorsWindow("Произошла ошибка аннулирования документа на удалённом сервере.", new List<string>(new string[] { errMessage }));
                    errorsWindow.ShowDialog();
                }
                catch (Exception ex)
                {
                    loadWindow.Close();
                    _dataBaseAdapter.Rollback();
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

                            var subject = _edoSystem.GetCertSubject();
                            var firstMiddleName = _cryptoUtil.ParseCertAttribute(subject, "G");
                            report.SignerPosition = _cryptoUtil.ParseCertAttribute(subject, "T");
                            report.SignerSurname = _cryptoUtil.ParseCertAttribute(subject, "SN");
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

                            if (_edoSystem.HasZipContent)
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
                            var signature = _cryptoUtil.Sign(contentBytes, true);

                            File.WriteAllBytes($"{edoFilesPath}//{SelectedItem.IdDocEdo}//{report.FileName}.xml", contentBytes);
                            File.WriteAllBytes($"{edoFilesPath}//{SelectedItem.IdDocEdo}//{report.FileName}.xml.sig", signature);

                            loadContext.SetLoadingText("Отправка");

                            if (_edoSystem.GetType() == typeof(DiadocEdoSystem))
                                _edoSystem.SendRevocationDocument(sellerReport.Function, contentBytes, signature, SelectedItem.IdDocEdo, SelectedItem.ParentEntityId);

                            if (SelectedItem.DocStatus == (int)DocEdoStatus.RevokeRequired)
                                SelectedItem.DocStatus = (int)DocEdoStatus.Revoked;
                            else
                                SelectedItem.DocStatus = (int)DocEdoStatus.RevokeRequested;

                            _dataBaseAdapter.Commit();

                            if(SelectedItem.DocStatus == (int)DocEdoStatus.Revoked)
                                loadContext.SetSuccessFullLoad("Документ успешно аннулирован.");
                            else
                                loadContext.SetSuccessFullLoad("Успешно выполнен запрос.");
                        }
                        catch (System.Net.WebException webEx)
                        {
                            _dataBaseAdapter.Rollback();
                            errorMessage = _log.GetRecursiveInnerException(webEx);
                            titleErrorText = "Произошла ошибка аннулирования документа на удалённом сервере.";
                        }
                        catch (Exception ex)
                        {
                            _dataBaseAdapter.Rollback();
                            errorMessage = _log.GetRecursiveInnerException(ex);
                            titleErrorText = "Произошла ошибка аннулирования документа.";
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

            object[] parameters = null;
            string fileName = null;

            try
            {
                if (_edoSystem.GetType() == typeof(DiadocEdoSystem))
                {
                    parameters = new object[] { SelectedItem.IdDocEdo, SelectedItem.ParentEntityId };

                    string createDate = SelectedItem.CreateDate?.ToString("dd.MM.yyyy");

                    if (!string.IsNullOrEmpty(createDate))
                        fileName = $"УПД № {SelectedItem.Name} от {SelectedItem.CreateDate?.ToString("dd.MM.yyyy")}";
                    else
                        fileName = $"УПД № {SelectedItem.Name}";
                }
                else if (_edoSystem.GetType() == typeof(EdoLiteSystem))
                {
                    parameters = new object[] { SelectedItem.IdDocEdo };
                    fileName = SelectedItem.Name;
                }

                if (parameters == null)
                    return;

                var contentBytes = _edoSystem.GetDocumentPrintForm(parameters);

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

        private bool MarkedCodesOwnerCheck(IEnumerable<string> markedCodes, string ownerInn)
        {
            var positionInArray = 0;

            while (positionInArray < markedCodes.Count())
            {
                int length = markedCodes.Count() - positionInArray > 500 ? 500 : markedCodes.Count() - positionInArray;
                var markedCodesInfo = _honestMarkSystem.GetMarkedCodesInfo(ProductGroupsEnum.Perfumery, markedCodes.Skip(positionInArray).Take(length).ToArray());

                if (markedCodesInfo.Any(m => m?.CisInfo?.OwnerInn != ownerInn))
                    return false;

                positionInArray += 500;
            }

            return true;
        }

        private void SaveMarkedCodesToDataBase(byte[] sellerFileContent, decimal? idDoc = null, decimal? oldIdDoc = null)
        {
            var reporterDll = new Reporter.ReporterDll();
            var report = reporterDll.ParseDocument<Reporter.Reports.UniversalTransferSellerDocument>(sellerFileContent);
            string errorMessage = null;

            if (report.Products == null || report.Products.Count == 0)
                return;

            var markedCodes = new List<KeyValuePair<string, string>>();

            var productsWithTransportCodes = report?.Products?.Where(p => p.TransportPackingIdentificationCode != null && p.TransportPackingIdentificationCode.Count > 0);

            var transportCodes = productsWithTransportCodes?.SelectMany(p => p.TransportPackingIdentificationCode) ?? new List<string>();

            if (transportCodes.Count() > 0)
                _honestMarkSystem.GetCodesByThePiece(transportCodes, markedCodes);

            foreach (var product in report.Products)
                if (product.MarkedCodes != null && product.MarkedCodes.Count > 0)
                    _honestMarkSystem.GetCodesByThePiece(product.MarkedCodes, markedCodes);

            if (markedCodes.Count == 0)
                return;

            var markedCodesArray = markedCodes.Select(m => m.Key);

            if (!MarkedCodesOwnerCheck(markedCodesArray, SelectedItem.SenderInn))
                throw new Exception("В списке кодов маркировки есть не принадлежащие отправителю.");

            var productGroups = from markedCode in markedCodes
                                group markedCode by markedCode.Value into gr
                                join product in report.Products on gr.Key equals product.BarCode
                                select new { product.Quantity, product.BarCode, product.Description, product.Number,
                                    Count = gr.Count(), Items=gr.Select(g => g.Key).ToList() };

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
        }

        public void SaveParameters(params object[] parameters)
        {
            if (_edoSystem.GetType() == typeof(EdoLiteSystem))
            {
                ConfigSet.Configs.Config.GetInstance().EdoDocCount = (int)parameters[0];
                ConfigSet.Configs.Config.GetInstance().EdoLastDocDateTime = (DateTime)parameters[1];

                ConfigSet.Configs.Config.GetInstance().Save(ConfigSet.Configs.Config.GetInstance(), ConfigSet.Configs.Config.ConfFileName);
            }
            else if (_edoSystem.GetType() == typeof(DiadocEdoSystem))
            {
                ConfigSet.Configs.Config.GetInstance().EdoLastDocDateTime = (DateTime)parameters[0];
                ConfigSet.Configs.Config.GetInstance().Save(ConfigSet.Configs.Config.GetInstance(), ConfigSet.Configs.Config.ConfFileName);
            }
        }

        public List<IEdoSystemDocument<string>> GetNewDocuments(out object[] parameters)
        {
            if (_edoSystem.GetType() == typeof(EdoLiteSystem))
            {
                var docCount = _edoSystem.GetDocumentsCount();
                var edoDocCount = ConfigSet.Configs.Config.GetInstance()?.EdoDocCount ?? 0;

                parameters = new object[2] { docCount, DateTime.Now };
                if (edoDocCount < docCount)
                {
                    var documentList = _edoSystem.GetDocuments(DocumentInOutType.Inbox, docCount - edoDocCount,
                        ConfigSet.Configs.Config.GetInstance().EdoLastDocDateTime);

                    return documentList ?? new List<IEdoSystemDocument<string>>();
                }
                else
                    return new List<IEdoSystemDocument<string>>();
            }
            else if(_edoSystem.GetType() == typeof(DiadocEdoSystem))
            {
                parameters = new object[1] { DateTime.Now };

                var newDocuments = _edoSystem.GetDocuments(DocumentInOutType.Inbox, 0, ConfigSet.Configs.Config.GetInstance().EdoLastDocDateTime);
                return newDocuments;
            }
            else
            {
                parameters = null;
                return null;
            }
        }

        public bool SaveNewDocument(IEdoSystemDocument<string> document, out byte[] fileBytes)
        {
            if(!Directory.Exists(edoFilesPath))
                Directory.CreateDirectory(edoFilesPath);

            string dirPath = $"{edoFilesPath}//{document.EdoId}";
            Directory.CreateDirectory(dirPath);

            byte[] signature = null;

            if(_edoSystem.HasZipContent)
                fileBytes = _edoSystem.GetDocumentContent(document, DocumentInOutType.Inbox);
            else
                fileBytes = _edoSystem.GetDocumentContent(document, out signature, DocumentInOutType.Inbox);

            byte[] zipBytes;
            try
            {
                zipBytes = _edoSystem.GetZipContent(document.EdoId, DocumentInOutType.Inbox);
            }
            catch
            {
                zipBytes = null;
            }

            if (_edoSystem.GetType() == typeof(EdoLiteSystem))
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
            else if (_edoSystem.GetType() == typeof(DiadocEdoSystem))
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

        public void SaveOrgData(string orgInn, string orgName)
        {
            if(_dataBaseAdapter.GetType() == typeof(EdoLiteToDataBase))
                ((EdoLiteToDataBase)_dataBaseAdapter).SaveOrgData(orgInn, orgName);
            else if(_dataBaseAdapter.GetType() == typeof(DiadocEdoToDataBase))
            {
                var edoSystem = _edoSystem as DiadocEdoSystem;

                if (edoSystem == null)
                    return;

                var counteragentsBoxIdsForOrganization = edoSystem.GetCounteragentsBoxesForOrganization(orgInn);
                ((DiadocEdoToDataBase)_dataBaseAdapter).SetPermittedBoxIds(counteragentsBoxIdsForOrganization);

                var orgKpp = edoSystem.GetKppForMyOrganization(orgInn);
                ((DiadocEdoToDataBase)_dataBaseAdapter).SetOrgData(orgName, orgInn, orgKpp);
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
                    SelectedDetail.IdGood = refGoodsModel.SelectedItem.Id;
                    OnPropertyChanged("SelectedDetail");
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

        protected override void OnDispose()
        {
            _dataBaseAdapter.Dispose();
        }
    }
}
