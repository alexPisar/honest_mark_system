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
    public class EdoLiteToDataBase : IEdoDataBaseAdapter<AbtDbContext>
    {
        private const string providerName = "EDO LITE";

        private string _orgName;
        private string _orgInn;
        private string _dataBaseUser;
        private AbtDbContext _abt;
        private List<DocEdoPurchasing> _documents;
        private List<string> _permittedSenderInnsForUser;

        public void InitializeContext()
        {
            _abt = new AbtDbContext();

            _dataBaseUser = ConfigSet.Configs.Config.GetInstance().DataBaseUser;
            _permittedSenderInnsForUser = (from refUser in _abt.RefUsersByEdoShippers
                            join cus in _abt.RefCustomers
                            on refUser.IdCustomer equals cus.Id
                            where refUser.UserName == _dataBaseUser
                            select cus.Inn).ToList() ?? new List<string>();

            _documents = _abt.DocEdoPurchasings
                .Where(d => d.EdoProviderName == providerName && d.ReceiverInn == _orgInn)
                .ToList();
        }

        public bool DocumentCanBeAddedByUser(IEdoSystemDocument<string> document)
        {
            var doc = (EdoLiteDocuments)document;

            return _permittedSenderInnsForUser.Exists(p => p == (doc?.Sender?.Inn.ToString() ?? ""));
        }

        public object AddDocumentToDataBase(IEdoSystemDocument<string> document, byte[] content, WebSystems.DocumentInOutType inOutType = WebSystems.DocumentInOutType.None)
        {
            var doc = (EdoLiteDocuments)document;

            var reporterDll = new Reporter.ReporterDll();
            var report = reporterDll.ParseDocument<UniversalTransferSellerDocument>(content);

            var newDocInDb = new DocEdoPurchasing()
            {
                IdDocEdo = doc.EdoId,
                EdoProviderName = providerName,
                CreateDate = doc.CreateDateTime,
                ReceiveDate = doc.Date,
                Name = doc.Number,
                TotalPrice = doc.TotalPrice,
                TotalVatAmount = doc.TotalVatAmount,
                IdDocType = doc.DocType,
                SenderEdoId = report.SenderEdoId,
                ReceiverEdoId = report.ReceiverEdoId,
                SenderEdoOrgName = report.EdoProviderOrgName,
                SenderEdoOrgInn = report.ProviderInn,
                SenderEdoOrgId = report.EdoId,
                FileName = report.FileName,
                UserName = _dataBaseUser
            };

            if (doc.Status == (int)WebSystems.EdoLiteDocumentStatus.SignedAndSend)
            {
                newDocInDb.DocStatus = (int)WebSystems.DocEdoStatus.Processed;
            }
            else if (doc.Status == (int)WebSystems.EdoLiteDocumentStatus.Delivered
                || doc.Status == (int)WebSystems.EdoLiteDocumentStatus.Viewed)
            {
                newDocInDb.DocStatus = (int)WebSystems.DocEdoStatus.NoSignatureRequired;
            }
            else if (doc.Status == (int)WebSystems.EdoLiteDocumentStatus.Sent)
            {
                newDocInDb.DocStatus = (int)WebSystems.DocEdoStatus.Sent;
            }
            else if (doc.Status == (int)WebSystems.EdoLiteDocumentStatus.SignatureError
                || doc.Status == (int)WebSystems.EdoLiteDocumentStatus.DeliveryError)
            {
                newDocInDb.DocStatus = (int)WebSystems.DocEdoStatus.ProcessingError;
            }
            else
            {
                newDocInDb.DocStatus = (int)WebSystems.DocEdoStatus.New;
            }

            if (inOutType == WebSystems.DocumentInOutType.Inbox)
            {
                newDocInDb.SenderInn = doc?.Sender?.Inn.ToString();
                newDocInDb.SenderName = doc?.Sender?.Name;
                newDocInDb.ReceiverInn = _orgInn;
                newDocInDb.ReceiverName = _orgName;
            }
            else if (inOutType == WebSystems.DocumentInOutType.Outbox)
            {
                newDocInDb.SenderInn = _orgInn;
                newDocInDb.SenderName = _orgName;
                newDocInDb.ReceiverInn = doc?.Recipient?.Inn.ToString();
                newDocInDb.ReceiverName = doc?.Recipient?.Name;
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
                    IdDocEdoPurchasing = newDocInDb.IdDocEdo,
                    EdoDocument = newDocInDb,
                    DetailNumber = product.Number
                };

                var refGoods = _abt.RefBarCodes?
                            .Where(b => b.BarCode == newDetail.BarCode && b.IsPrimary == false)?
                            .Select(b => b.IdGood)?.Distinct()?.ToList() ?? new List<decimal?>();

                if (refGoods.Count == 1)
                    newDetail.IdGood = refGoods.First();

                newDocInDb.Details.Add(newDetail);
            }

            _abt.DocEdoPurchasings.Add(newDocInDb);
            _documents.Add(newDocInDb);

            return newDocInDb;
        }

        public bool ExistsDocumentInDataBase(IEdoSystemDocument<string> document)
        {
            return _documents.Exists(d => d.IdDocEdo == document.EdoId);
        }

        public object GetDocumentFromDb(IEdoSystemDocument<string> document)
        {
            return _documents.FirstOrDefault(d => d.IdDocEdo == document.EdoId);
        }

        public object[] GetAllDocuments(DateTime dateFrom, DateTime dateTo)
        {
            return _documents.Where(d => _permittedSenderInnsForUser.FirstOrDefault(c => c == d.SenderInn) != null && d.CreateDate > dateFrom && d.CreateDate <= dateTo)
                .ToArray();
        }

        public System.Collections.IEnumerable GetJournalDocuments(object selectedDocument)
        {
            var docEdoDocument = (DocEdoPurchasing)selectedDocument;

            return from doc in _abt.DocJournals where doc.DocGoods != null && (doc.IdDocType == (int)DataContextManagementUnit.DataAccess.DocJournalType.Receipt || doc.IdDocType == (int)DataContextManagementUnit.DataAccess.DocJournalType.Translocation)
                   join docGood in _abt.DocGoods on doc.Id equals docGood.IdDoc
                   join seller in _abt.RefCustomers on docGood.IdSeller equals seller.IdContractor
                   where seller.Inn == docEdoDocument.SenderInn select doc;
        }

        public object GetPurchasingDocumentById(decimal idDocPurchasing)
        {
            return _abt.DocPurchasings.FirstOrDefault(d => d.Id == idDocPurchasing);
        }

        public void SaveOrgData(string orgInn, string orgName)
        {
            _orgInn = orgInn;
            _orgName = orgName;
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
            }).ToList();

            foreach(var label in labels)
                _abt.DocGoodsDetailsLabels.Add(label);
        }

        public void AddMarkedCodes(decimal idDocJournal, List<KeyValuePair<decimal, List<string>>> markedCodesByGoods, IEnumerable<string> updatedCodes = null)
        {
            var labels = markedCodesByGoods.SelectMany(m => m.Value.Select(s => new DocGoodsDetailsLabels
            {
                IdDoc = idDocJournal,
                DmLabel = s,
                IdGood = m.Key,
                InsertDateTime = DateTime.Now
            }));

            if (updatedCodes != null && updatedCodes?.Count() > 0)
                labels = labels.Where(l => !updatedCodes.Any(u => u == l.DmLabel));

            if (labels.Count() == 0)
                return;

            labels = labels.Where(label => !_abt.DocGoodsDetailsLabels.Any(l => l.IdDoc == idDocJournal /*&& l.IdGood == label.IdGood*/ && l.DmLabel == label.DmLabel));

            if (labels.Count() == 0)
                return;

            _abt.DocGoodsDetailsLabels.AddRange(labels);
        }

        public IEnumerable<string> UpdateCodes(decimal idDocJournal, IEnumerable<string> markedCodes, decimal? oldIdDocJournal = null)
        {
            IEnumerable<DocGoodsDetailsLabels> labels = null;

            if (oldIdDocJournal != null)
                labels = _abt.DocGoodsDetailsLabels.Where(l => l.IdDoc == oldIdDocJournal && markedCodes.Any(m => m == l.DmLabel));
            else
                labels = _abt.DocGoodsDetailsLabels.Where(l => markedCodes.Any(m => m == l.DmLabel));

            if (labels.Count() == 0)
                return new List<string>();

            foreach (var label in labels)
                label.IdDoc = idDocJournal;

            return labels.Select(l => l.DmLabel);
        }

        public void UpdateMarkedCodeIncomingStatuses(decimal idDocJournal, WebSystems.MarkedCodeComingStatus status)
        {
            var docJournal = _abt.DocJournals.First(d => d.Id == idDocJournal);

            if (docJournal.IdDocType == (int)DataContextManagementUnit.DataAccess.DocJournalType.Receipt)
                _abt.Database.ExecuteSqlCommand($"UPDATE doc_goods_details_labels SET LABEL_STATUS = {(int)status}, POST_DATETIME = sysdate where id_doc = {idDocJournal}");
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
            document.IdDocJournal = idDoc.Value;
            return idDoc.Value;
        }

        public bool IsExistsNotReceivedCodes(decimal idDoc)
        {
            var count = _abt.Database.SqlQuery<int>($"select count(*) from doc_goods_details_labels where id_doc = {idDoc} and LABEL_STATUS <> 1").First();

            return count > 0;
        }

        public System.Data.Entity.DbContextTransaction BeginTransaction()
        {
            return _abt.Database.BeginTransaction();
        }

        public void ReloadEntry(object entry)
        {
            _abt.Entry(entry)?.Reload();
        }

        public void Commit(System.Data.Entity.DbContextTransaction transaction = null)
        {
            _abt.SaveChanges();
            transaction?.Commit();
        }

        public void Dispose()
        {
            _abt?.Dispose();
            _abt = null;
        }
    }
}
