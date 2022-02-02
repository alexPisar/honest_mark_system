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

        private IEdoSystem _edoSystem;
        private Interfaces.IEdoDataBaseAdapter<AbtDbContext> _dataBaseAdapter;
        private UtilitesLibrary.Service.CryptoUtil _cryptoUtil;
        private WebSystems.Systems.HonestMarkSystem _honestMarkSystem;

        public List<DocEdoPurchasingDetail> Details => SelectedItem?.Details;
        public DocEdoPurchasingDetail SelectedDetail { get; set; }

        public DateTime DateTo { get; set; } = DateTime.Now.AddDays(1);
        public DateTime DateFrom { get; set; } = DateTime.Now.AddMonths(-6);

        public override RelayCommand RefreshCommand => new RelayCommand((o) => { Refresh(); });
        public override RelayCommand EditCommand => new RelayCommand((o) => { Save(); });
        public RelayCommand ChangePurchasingDocumentCommand => new RelayCommand((o) => { ChangePurchasingDocument(); });
        public RelayCommand SignAndSendCommand => new RelayCommand((o) => { SignAndSend(); });
        public RelayCommand ExportToTraderCommand => new RelayCommand((o) => { ExportToTrader(); });
        public RelayCommand WithdrawalCodesCommand => new RelayCommand((o) => { WithdrawalCodes(); });

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
                    var docProcessingInfo = _honestMarkSystem.GetEdoDocumentProcessInfo(processingDocument.FileName);

                    if (docProcessingInfo.Code == EdoLiteProcessResultStatus.SUCCESS)
                    {
                        processingDocument.DocStatus = (int)DocEdoStatus.Processed;
                        LoadStatus(processingDocument);
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

                if(processingDocuments.Exists(p => p.DocStatus != (int)DocEdoStatus.Sent))
                    _dataBaseAdapter.Commit();
            }
            catch (Exception ex)
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

        private void ChangePurchasingDocument()
        {
            if(SelectedItem == null)
            {
                System.Windows.MessageBox.Show(
                    "Не выбран документ для сопоставления.", "Ошибка", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                return;
            }

            var docs = _dataBaseAdapter.GetPurchasingDocuments();

            var docPurchasingWindow = new PurchasingDocumentsWindow();
            var docPurchasingModel = new PurchasingDocumentsModel(docs.Cast<DocPurchasing>());

            if (SelectedItem.IdDocPurchasing != null)
                docPurchasingModel.SetChangedDocument(SelectedItem.IdDocPurchasing.Value);

            docPurchasingWindow.DataContext = docPurchasingModel;
            if(docPurchasingWindow.ShowDialog() == true)
            {
                try
                {
                    SelectedItem.IdDocPurchasing = docPurchasingModel.SelectedItem.Id;
                    _dataBaseAdapter.Commit();
                    OnPropertyChanged("SelectedItem");
                }
                catch(Exception ex)
                {
                    _dataBaseAdapter.Rollback();
                    string errorMessage = _log.GetRecursiveInnerException(ex);
                    _log.Log(errorMessage);

                    var errorsWindow = new ErrorsWindow("Произошла ошибка сопоставления.", new List<string>(new string[] { errorMessage }));
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

            if (SelectedItem.DocStatus == (int)DocEdoStatus.NoSignatureRequired)
            {
                System.Windows.MessageBox.Show(
                    "Данный документ не предназначен для подписания.", "Ошибка", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                return;
            }

            if (SelectedItem.IdDocPurchasing == null)
            {
                System.Windows.MessageBox.Show(
                    "Данный документ не сопоставлен с документом закупок.", "Ошибка", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
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
            signWindow.Report.EdoProgramVersion = _edoSystem.ProgramVersion;

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
                        var xml = reportForSend.GetXmlContent();
                        var fileBytes = Encoding.GetEncoding(1251).GetBytes(xml);
                        var signature = _cryptoUtil.Sign(fileBytes, true);

                        File.WriteAllBytes($"{edoFilesPath}//{SelectedItem.IdDocEdo}//{reportForSend.FileName}.xml", fileBytes);
                        File.WriteAllBytes($"{edoFilesPath}//{SelectedItem.IdDocEdo}//{reportForSend.FileName}.xml.sig", signature);

                        var directory = new DirectoryInfo(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location));

                        loadContext.SetLoadingText("Сохранение в базе данных");
                        SaveMarkedCodesToDataBase(docSellerContent);

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

                            if (SelectedItem.IdDocType == (int)Diadoc.Api.Proto.DocumentType.UniversalTransferDocument)
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

        private void ExportToTrader()
        {

        }

        private void WithdrawalCodes()
        {
            var markedCodes = _dataBaseAdapter.GetAllMarkedCodes().Cast<DocGoodsDetailsLabels>().ToList();

            var withdrawalWindow = new ChangeMarkedCodesWindow();
            withdrawalWindow.DataContext = new ChangeMarkedCodesModel(markedCodes, _honestMarkSystem);
            withdrawalWindow.ShowDialog();
        }

        private void SaveMarkedCodesToDataBase(byte[] sellerFileContent)
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

            var docPurchasing = _dataBaseAdapter.GetPurchasingDocumentById(SelectedItem.IdDocPurchasing.Value);

            if (docPurchasing == null)
                throw new Exception("Не найден документ закупок в базе.");

            if (((DocPurchasing)docPurchasing)?.IdDocLink == null)
                throw new Exception("Для документа закупок не найден трейдер документ.");

            productGroups = productGroups.ToList();
            foreach (var productGroup in productGroups)
            {
                var detail = Details?.FirstOrDefault(d => d.DetailNumber == productGroup.Number);

                if (detail == null)
                    throw new Exception($"Не найден товар с названием {productGroup.Description}.");

                _dataBaseAdapter.AddMarkedCodes(((DocPurchasing)docPurchasing).IdDocLink.Value, detail.IdGood.Value, productGroup.Items);
            }
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
                    doc.DocumentType != Diadoc.Api.Proto.DocumentType.XmlTorg12 && doc.DocumentType != Diadoc.Api.Proto.DocumentType.UniversalTransferDocument)
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
                ((DiadocEdoToDataBase)_dataBaseAdapter).SetOrgData(orgName, orgInn);
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
    }
}
