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
using HonestMarkSystem.Implementations;

namespace HonestMarkSystem.Models
{
    public class MainViewModel : ListViewModel<DocEdoPurchasing>, Interfaces.IEdoDocumentsView
    {
        private const string edoFilesPath = "Files";

        private IEdoSystem _edoSystem;
        private Interfaces.IEdoDataBaseAdapter<AbtDbContext> _dataBaseAdapter;

        public override RelayCommand RefreshCommand => new RelayCommand((o) => { Refresh(); });
        public RelayCommand ChangePurchasingDocumentCommand => new RelayCommand((o) => { ChangePurchasingDocument(); });

        public MainViewModel()
        {
            _edoSystem = new EdoLiteSystem();
            _dataBaseAdapter = new EdoLiteToDataBase();
            _dataBaseAdapter.InitializeContext();

            ItemsList = new System.Collections.ObjectModel.ObservableCollection<DocEdoPurchasing>();
        }

        private void Refresh()
        {
            object[] parameters;
            var documents = GetNewDocuments(out parameters);

            if (documents.Count > 0)
            {
                try
                {
                    foreach (var doc in documents)
                    {
                        SaveNewDocument(doc);

                        var docFromDb = (DocEdoPurchasing)_dataBaseAdapter.GetDocumentFromDb(doc);

                        if (docFromDb == null)
                            _dataBaseAdapter.AddDocumentToDataBase(doc, DocumentInOutType.Inbox);
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
            else
            {
                parameters = null;
                return null;
            }
        }

        public void SaveNewDocument(IEdoSystemDocument<string> document)
        {
            if(!Directory.Exists(edoFilesPath))
                Directory.CreateDirectory(edoFilesPath);

            string dirPath = $"{edoFilesPath}//{document.EdoId}";
            Directory.CreateDirectory(dirPath);

            var fileBytes = _edoSystem.GetDocumentContent(document, DocumentInOutType.Inbox);

            if (_edoSystem.GetType() == typeof(EdoLiteSystem))
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
                }
        }

        public void SaveOrgData(string orgInn, string orgName)
        {
            if(_dataBaseAdapter.GetType() == typeof(EdoLiteToDataBase))
                ((EdoLiteToDataBase)_dataBaseAdapter).SaveOrgData(orgInn, orgName);
        }
    }
}
