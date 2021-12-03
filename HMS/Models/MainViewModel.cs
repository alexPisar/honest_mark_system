﻿using System;
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

        public override RelayCommand RefreshCommand => new RelayCommand((o) => { Refresh(); });
        public RelayCommand ChangePurchasingDocumentCommand => new RelayCommand((o) => { ChangePurchasingDocument(); });
        public RelayCommand SignAndSendCommand => new RelayCommand((o) => { SignAndSend(); });

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

                var errorsWindow = new ErrorsWindow("Произошола ошибка получения документов на удалённом сервере.", new List<string>(new string[] { errorMessage }));
                errorsWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                string errorMessage = _log.GetRecursiveInnerException(ex);
                _log.Log(errorMessage);

                var errorsWindow = new ErrorsWindow("Произошола ошибка получения документов.", new List<string>(new string[] { errorMessage }));
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

            var docs = _dataBaseAdapter.GetAllDocuments().Cast<DocEdoPurchasing>();
            ItemsList = new System.Collections.ObjectModel.ObservableCollection<DocEdoPurchasing>(docs);
            SelectedItem = null;

            try
            {
                var processingDocuments = docs.Where(d => d.DocStatus == (int)DocEdoStatus.Sent).ToList();

                foreach (var processingDocument in processingDocuments)
                {
                    var docProcessingInfo = _honestMarkSystem.GetEdoDocumentProcessInfo(processingDocument.FileName);

                    if (docProcessingInfo.Code == EdoLiteProcessResultStatus.SUCCESS)
                        processingDocument.DocStatus = (int)DocEdoStatus.Processed;
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

                            SelectedItem.SignatureFileName = reportForSend.FileName;

                            if(reportForSend.FileName.StartsWith("ON_NSCHFDOPPOKMARK"))
                                SelectedItem.DocStatus = (int)DocEdoStatus.Sent;
                            else
                                SelectedItem.DocStatus = (int)DocEdoStatus.Processed;

                            UpdateProperties();
                        }

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

        private void SaveMarkedCodesToDataBase(byte[] sellerFileContent)
        {
            var reporterDll = new Reporter.ReporterDll();
            var report = reporterDll.ParseDocument<Reporter.Reports.UniversalTransferSellerDocument>(sellerFileContent);

            if (report.Products == null || report.Products.Count == 0)
                return;

            var markedCodes = new List<KeyValuePair<string, string>>();
            foreach (var product in report.Products)
            {
                if(product.MarkedCodes != null && product.MarkedCodes.Count > 0)
                    _honestMarkSystem.GetCodesByThePiece(product.MarkedCodes, markedCodes);

                if (product.TransportPackingIdentificationCode != null && product.TransportPackingIdentificationCode.Count > 0)
                    _honestMarkSystem.GetCodesByThePiece(product.TransportPackingIdentificationCode, markedCodes);
            }

            _dataBaseAdapter.SaveMarkedCodes(SelectedItem.IdDocPurchasing.Value, markedCodes.ToArray());
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
    }
}
