using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataContextManagementUnit.DataAccess.Contexts.Abt;
using HonestMarkSystem.Interfaces;
using WebSystems.Models;
using Reporter.Reports;

namespace HonestMarkSystem.Implementations
{
    public class DiadocEdoToDataBase : IEdoDataBaseAdapter<AbtDbContext>
    {
        private const string providerName = "DIADOC";

        private string _dataBaseUser;
        private string _orgName;
        private string _orgInn;
        private string _orgKpp;
        private AbtDbContext _abt;
        private List<Diadoc.Api.Proto.Box> _permittedBoxes;
        private List<DocEdoPurchasing> _documents;

        public void InitializeContext()
        {
            _abt = new AbtDbContext();

            _dataBaseUser = ConfigSet.Configs.Config.GetInstance().DataBaseUser;

            _documents = _abt.DocEdoPurchasings
                .Where(d => d.EdoProviderName == providerName && d.ReceiverInn == _orgInn)
                .ToList();

            var nlsNumericCharacters = _abt.SelectSingleValue("select value from v$nls_parameters where parameter = 'NLS_NUMERIC_CHARACTERS'");

            if (nlsNumericCharacters != ".,")
                _abt.Database.ExecuteSqlCommand("ALTER SESSION SET NLS_LANGUAGE = 'AMERICAN' NLS_NUMERIC_CHARACTERS= '.,'");
        }

        public void SetPermittedBoxIds(List<KeyValuePair<Diadoc.Api.Proto.Box, Diadoc.Api.Proto.Organization>> boxesByInn)
        {
            var permittedSenderInnsForUser = (from refUser in _abt.RefUsersByEdoShippers
                                              join cus in _abt.RefCustomers on refUser.IdCustomer equals cus.Id
                                              where refUser.UserName == _dataBaseUser
                                              select cus.Inn)?.ToList() ?? new List<string>();
            //permittedSenderInnsForUser.Add("9652306541");

            _permittedBoxes = (from box in boxesByInn
                              where permittedSenderInnsForUser.Exists(p => p == box.Value.Inn)
                              select new Diadoc.Api.Proto.Box
                              {
                                  BoxId = box.Key?.BoxId,
                                  Title = box.Key?.Title,
                                  BoxIdGuid = box.Key?.BoxIdGuid,
                                  Organization = box.Value
                              }).ToList();
        }

        public bool DocumentCanBeAddedByUser(IEdoSystemDocument<string> document)
        {
            var doc = document as DiadocEdoDocument;

            return _permittedBoxes?.Exists(p => p.BoxId == doc.CounteragentBoxId) ?? false;
        }

        public void SetOrgData(string orgName, string inn, string kpp)
        {
            _orgName = orgName;
            _orgInn = inn;
            _orgKpp = kpp;
        }

        public object[] GetAllDocuments(DateTime dateFrom, DateTime dateTo)
        {
            return _documents.Where(d => _permittedBoxes.FirstOrDefault(c => c.BoxId == d.CounteragentEdoBoxId) != null && d.CreateDate > dateFrom && d.CreateDate <= dateTo)
                .ToArray();
        }

        public object[] GetPurchasingDocuments()
        {
            return _abt.DocPurchasings
                .Where(d => d.Firm != null && d.Firm.Customer != null && d.Firm.Customer.Inn == _orgInn)
                .ToArray();
        }

        public object GetPurchasingDocumentById(decimal idDocPurchasing)
        {
            return _abt.DocPurchasings.FirstOrDefault(d => d.Id == idDocPurchasing);
        }

        public void AddMarkedCode(decimal idDocJournal, decimal idGood, string markedCode)
        {
            if (_abt.DocGoodsDetailsLabels.FirstOrDefault(l => l.IdDoc == idDocJournal && l.IdGood == idGood && l.DmLabel == markedCode) != null)
                return;

            var label = new DocGoodsDetailsLabels
            {
                IdDoc = idDocJournal,
                DmLabel = markedCode,
                IdGood = idGood,
                InsertDateTime = DateTime.Now
            };

            _abt.DocGoodsDetailsLabels.Add(label);
        }

        public void AddMarkedCodes(decimal idDocJournal, decimal idGood, IEnumerable<string> markedCodes)
        {
            markedCodes = markedCodes.Where(m => !_abt.DocGoodsDetailsLabels.Any(l => l.IdDoc == idDocJournal && l.IdGood == idGood && l.DmLabel == m));

            if (markedCodes.Count() == 0)
                return;

            var labels = markedCodes.Select(m => new DocGoodsDetailsLabels
            {
                IdDoc = idDocJournal,
                DmLabel = m,
                IdGood = idGood,
                InsertDateTime = DateTime.Now
            });

            foreach(var label in labels)
                _abt.DocGoodsDetailsLabels.Add(label);
        }

        public void AddMarkedCodes(decimal idDocJournal, List<KeyValuePair<decimal, List<string>>> markedCodesByGoods)
        {
            var labels = markedCodesByGoods.SelectMany(m => m.Value.Select(s => new DocGoodsDetailsLabels
            {
                IdDoc = idDocJournal,
                DmLabel = s,
                IdGood = m.Key,
                InsertDateTime = DateTime.Now
            }));

            labels = labels.Where(label => !_abt.DocGoodsDetailsLabels.Any(l => l.IdDoc == idDocJournal && l.IdGood == label.IdGood && l.DmLabel == label.DmLabel));

            if (labels.Count() == 0)
                return;

            _abt.DocGoodsDetailsLabels.AddRange(labels);
        }

        public object AddDocumentToDataBase(IEdoSystemDocument<string> document, byte[] content, WebSystems.DocumentInOutType inOutType = WebSystems.DocumentInOutType.None)
        {
            var doc = document as DiadocEdoDocument;

            if (doc.DocumentType != Diadoc.Api.Proto.DocumentType.XmlAcceptanceCertificate && doc.DocumentType != Diadoc.Api.Proto.DocumentType.Invoice &&
                doc.DocumentType != Diadoc.Api.Proto.DocumentType.XmlTorg12 && doc.DocumentType != Diadoc.Api.Proto.DocumentType.UniversalTransferDocument &&
                doc.DocumentType != Diadoc.Api.Proto.DocumentType.UniversalTransferDocumentRevision)
                return null;

            var reporterDll = new Reporter.ReporterDll();
            var report = reporterDll.ParseDocument<UniversalTransferSellerDocument>(content);

            string total, vat;

            if(doc.DocumentType == Diadoc.Api.Proto.DocumentType.XmlAcceptanceCertificate)
            {
                total = doc.Document?.XmlAcceptanceCertificateMetadata?.Total;
                vat = doc.Document?.XmlAcceptanceCertificateMetadata?.Vat;
            }
            else if (doc.DocumentType == Diadoc.Api.Proto.DocumentType.Invoice)
            {
                total = doc.Document?.InvoiceMetadata?.Total;
                vat = doc.Document?.InvoiceMetadata?.Vat;
            }
            else if(doc.DocumentType == Diadoc.Api.Proto.DocumentType.XmlTorg12)
            {
                total = doc.Document?.XmlTorg12Metadata?.Total;
                vat = doc.Document?.XmlTorg12Metadata.Vat;
            }
            else if(doc.DocumentType == Diadoc.Api.Proto.DocumentType.UniversalTransferDocument)
            {
                total = doc.Document?.UniversalTransferDocumentMetadata?.Total;
                vat = doc.Document?.UniversalTransferDocumentMetadata?.Vat;
            }
            else if (doc.DocumentType == Diadoc.Api.Proto.DocumentType.UniversalTransferDocumentRevision)
            {
                total = doc.Document?.UniversalTransferDocumentRevisionMetadata?.Total;
                vat = doc.Document?.UniversalTransferDocumentRevisionMetadata?.Vat;
            }
            else
            {
                total = null;
                vat = null;
            }

            var newDoc = new DocEdoPurchasing
            {
                IdDocEdo = doc.EdoId,
                EdoProviderName = providerName,
                Name = doc.Title,
                IdDocType = (int)doc.DocumentType,
                CounteragentEdoBoxId = doc.CounteragentBoxId,
                ParentEntityId = doc.EntityId,
                ReceiveDate = doc.DeliveryDate,
                CreateDate = doc.CreatedDate,
                TotalPrice = total,
                TotalVatAmount = vat,
                SenderEdoId = report.SenderEdoId,
                ReceiverEdoId = report.ReceiverEdoId,
                SenderEdoOrgName = report.EdoProviderOrgName,
                SenderEdoOrgInn = report.ProviderInn,
                SenderEdoOrgId = report.EdoId,
                FileName = report.FileName,
                UserName = _dataBaseUser
            };

            var box = _permittedBoxes?.FirstOrDefault(p => p.BoxId == doc.CounteragentBoxId);

            if (box == null)
                throw new Exception($"Ящик пользователя {doc.CounteragentBoxId} не найден среди разрешённых.");

            var counteragentName = box.Organization?.FullName;
            var counteragentInn = box.Organization?.Inn;
            var counteragentKpp = box.Organization?.Kpp;

            if (inOutType == WebSystems.DocumentInOutType.Inbox)
            {
                newDoc.SenderInn = counteragentInn;
                newDoc.SenderName = counteragentName;
                newDoc.SenderKpp = counteragentKpp;
                newDoc.ReceiverInn = _orgInn;
                newDoc.ReceiverKpp = _orgKpp;
                newDoc.ReceiverName = _orgName;
            }
            else if (inOutType == WebSystems.DocumentInOutType.Outbox)
            {
                newDoc.SenderInn = _orgInn;
                newDoc.SenderKpp = _orgKpp;
                newDoc.SenderName = _orgName;
                newDoc.ReceiverInn = counteragentInn;
                newDoc.ReceiverName = counteragentName;
                newDoc.ReceiverKpp = counteragentKpp;
            }

            if (doc.DocStatus == "Success")
                newDoc.DocStatus = (int)WebSystems.DocEdoStatus.Processed;
            else if (doc.DocStatus == "Error")
            {
                newDoc.DocStatus = (int)WebSystems.DocEdoStatus.ProcessingError;
                newDoc.ErrorMessage = doc.DocStatusText;
            }
            else
                newDoc.DocStatus = (int)WebSystems.DocEdoStatus.New;

            if(doc.DocumentType == Diadoc.Api.Proto.DocumentType.UniversalTransferDocumentRevision)
            {
                newDoc.Name = $"Исправление {report.DocNumber} № {newDoc.Name}";

                var parent = _documents.FirstOrDefault(d => d.Name == report.DocNumber 
                && d.SenderEdoId == newDoc.SenderEdoId && d.ReceiverEdoId == newDoc.ReceiverEdoId);

                if(parent != null)
                {
                    newDoc.Parent = parent;
                    newDoc.ParentIdDocEdo = parent.IdDocEdo;
                    parent.Children.Add(newDoc);
                }
            }

            foreach(var product in report.Products)
            {
                var newDetail = new DocEdoPurchasingDetail
                {
                    BarCode = product.BarCode,
                    Quantity = product.Quantity,
                    Description = product.Description,
                    Price = product.Price,
                    Subtotal = product.Subtotal,
                    TaxAmount = product.TaxAmount,
                    IdDocEdoPurchasing = newDoc.IdDocEdo,
                    EdoDocument = newDoc,
                    DetailNumber = product.Number
                };

                var refGoods = _abt.RefBarCodes?
                            .Where(b => b.BarCode == newDetail.BarCode && b.IsPrimary == false)?
                            .Select(b => b.IdGood)?.Distinct()?.ToList() ?? new List<decimal?>();

                if (refGoods.Count == 1)
                    newDetail.IdGood = refGoods.First();
                else if(newDoc.Parent != null)
                {
                    var curDetail = newDoc.Parent.Details.FirstOrDefault(d => d.BarCode == newDetail.BarCode);

                    if (curDetail != null)
                        newDetail.IdGood = curDetail.IdGood;
                }

                newDoc.Details.Add(newDetail);
            }

            _abt.DocEdoPurchasings.Add(newDoc);
            _documents.Add(newDoc);

            if(!_abt.Entry(newDoc).Reference("Status").IsLoaded)
                _abt.Entry(newDoc).Reference("Status").Load();

            return newDoc;
        }

        public bool ExistsDocumentInDataBase(IEdoSystemDocument<string> document)
        {
            return _documents.Exists(d => d.IdDocEdo == document.EdoId);
        }

        public object GetDocumentFromDb(IEdoSystemDocument<string> document)
        {
            return _documents.FirstOrDefault(d => d.IdDocEdo == document.EdoId);
        }

        public void LoadStatus(DocEdoPurchasing doc)
        {
            doc.Status = null;
            _abt.Entry(doc).Reference("Status").IsLoaded = false;
            _abt.Entry(doc).Reference("Status").Load();
        }

        public List<object> GetRefGoodsByBarCode(string barCode)
        {
            var refGoods = (from refBarCode in _abt.RefBarCodes
                            where refBarCode.BarCode == barCode
                            join refGood in _abt.RefGoods
                            on refBarCode.IdGood equals refGood.Id
                            select refGood)?.ToList<object>() ?? new List<object>();

            return refGoods;
        }

        public List<object> GetAllRefGoods()
        {
            return _abt.RefGoods.ToList<object>();
        }

        public List<object> GetAllMarkedCodes()
        {
            return _abt.DocGoodsDetailsLabels.Where(l => l.DmLabel.Length == 31 && l.SaleDmLabel == null).ToList<object>();
        }

        public IEnumerable<string> GetMarkedCodesByDocumentId(decimal? docJournalId)
        {
            if (docJournalId == null)
                return null;

            return _abt.DocGoodsDetailsLabels.Where(l => l.IdDoc == docJournalId).Select(l => l.DmLabel);
        }

        public decimal ExportDocument(object documentObject)
        {
            var document = (DocEdoPurchasing)documentObject;

            var sender = (from senderCustomer in _abt.RefCustomers
                          where senderCustomer.Inn == document.SenderInn && senderCustomer.IdContractor != null
                          join refUser in _abt.RefUsersByEdoShippers
                          on senderCustomer.Id equals (refUser.IdCustomer)
                          where refUser.UserName == _dataBaseUser
                          select senderCustomer).FirstOrDefault();

            if (sender.IdContractor == null)
                throw new Exception("Данный отправитель не задан в базе");

            var receiver = (from receiverCustomer in _abt.RefCustomers
                            where receiverCustomer.Inn == document.ReceiverInn && receiverCustomer.IdContractor != null
                            select receiverCustomer).FirstOrDefault();

            if (receiver.IdContractor == null)
                throw new Exception("Данный получатель не задан в базе");

            decimal? idDoc = null;

            var parameters =
                new object[] {
                    new Oracle.ManagedDataAccess.Client.OracleParameter("p_comments", Oracle.ManagedDataAccess.Client.OracleDbType.Varchar2, string.Empty, System.Data.ParameterDirection.Input),
                    new Oracle.ManagedDataAccess.Client.OracleParameter("p_id_seller", sender.IdContractor),
                    new Oracle.ManagedDataAccess.Client.OracleParameter("p_id_customer", receiver.IdContractor),
                    new Oracle.ManagedDataAccess.Client.OracleParameter("p_id", Oracle.ManagedDataAccess.Client.OracleDbType.Decimal, System.Data.ParameterDirection.Output),
                    new Oracle.ManagedDataAccess.Client.OracleParameter("p_code", Oracle.ManagedDataAccess.Client.OracleDbType.Varchar2, System.Data.ParameterDirection.Output){ Size=20 } };

            _abt.ExecuteProcedure("abt.Add_document", parameters);
            idDoc = ((Oracle.ManagedDataAccess.Types.OracleDecimal)((Oracle.ManagedDataAccess.Client.OracleParameter)parameters[3]).Value).Value;

            if (idDoc == null)
                throw new Exception("Не удалось получить идентификатор созданного документа.");

            foreach (var detail in document.Details)
            {
                parameters = new object[]
                {
                    new Oracle.ManagedDataAccess.Client.OracleParameter("p_id_doc", idDoc),
                    new Oracle.ManagedDataAccess.Client.OracleParameter("p_id_good", detail.IdGood),
                    new Oracle.ManagedDataAccess.Client.OracleParameter("p_quantity", detail.Quantity)
                };

                _abt.ExecuteProcedure("ABT.Add_document_row", parameters);
            }

            parameters = new object[]
            {
                new Oracle.ManagedDataAccess.Client.OracleParameter("p_id_doc", idDoc),
                new Oracle.ManagedDataAccess.Client.OracleParameter("p_id_doc_edo", Oracle.ManagedDataAccess.Client.OracleDbType.Varchar2, document.IdDocEdo, System.Data.ParameterDirection.Input),
                new Oracle.ManagedDataAccess.Client.OracleParameter("p_id", Oracle.ManagedDataAccess.Client.OracleDbType.Decimal, System.Data.ParameterDirection.Output)
            };

            _abt.ExecuteProcedure("ABT.Add_document_purchasing", parameters);
            document.IdDocPurchasing = ((Oracle.ManagedDataAccess.Types.OracleDecimal)((Oracle.ManagedDataAccess.Client.OracleParameter)parameters[2]).Value).Value;
            return idDoc.Value;
        }

        public void Commit()
        {
            _abt.SaveChanges();
        }

        public void Rollback()
        {
            InitializeContext();
        }
    }
}
