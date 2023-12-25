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
        private AbtDbContext _abt = null;
        private List<Diadoc.Api.Proto.Box> _permittedBoxes;
        private List<DocEdoPurchasing> _documents;

        public DiadocEdoToDataBase()
        {
            _permittedBoxes = new List<Diadoc.Api.Proto.Box>();
        }

        public void InitializeContext()
        {
            _abt = new AbtDbContext();

            _dataBaseUser = ConfigSet.Configs.Config.GetInstance().DataBaseUser;

            _documents = _abt.DocEdoPurchasings
                .Where(d => d.EdoProviderName == providerName)
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

            var permittedBoxes = (from box in boxesByInn
                              where permittedSenderInnsForUser.Exists(p => p == box.Value.Inn) && box.Key?.BoxId != null && !_permittedBoxes.Any(p => p.BoxId == box.Key.BoxId)
                                  select new Diadoc.Api.Proto.Box
                              {
                                  BoxId = box.Key?.BoxId,
                                  Title = box.Key?.Title,
                                  BoxIdGuid = box.Key?.BoxIdGuid,
                                  Organization = box.Value
                              }).ToList();

            _permittedBoxes.AddRange(permittedBoxes);
        }

        public bool DocumentCanBeAddedByUser(Models.ConsignorOrganization myOrganization, IEdoSystemDocument<string> document)
        {
            var doc = document as DiadocEdoDocument;

            var box = _permittedBoxes?.FirstOrDefault(p => p.BoxId == doc.CounteragentBoxId);

            if (box == null)
                return false;

            return myOrganization.ShipperOrgInns.Exists(s => s == box.Organization?.Inn);
        }

        public object[] GetAllDocuments(DateTime dateFrom, DateTime dateTo)
        {
            return _documents.Where(d => _permittedBoxes.FirstOrDefault(c => c.BoxId == d.CounteragentEdoBoxId) != null && d.CreateDate > dateFrom && d.CreateDate <= dateTo)
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

        public System.Collections.IEnumerable GetJournalMarkedDocumentsByType(int docType)
        {
            return from doc in _abt.DocJournals
                   where doc.DocGoods != null && doc.IdDocType == docType
                   join docGood in _abt.DocGoods on doc.Id equals docGood.IdDoc
                   where (from label in _abt.DocGoodsDetailsLabels where label.IdDocReturn == doc.Id select label).Any()
                   select doc;
        }

        public object GetPurchasingDocumentById(decimal idDocPurchasing)
        {
            return _abt.DocPurchasings.FirstOrDefault(d => d.Id == idDocPurchasing);
        }

        public object GetDocJournal(decimal idDocJournal)
        {
            var docJournal = _abt.DocJournals.FirstOrDefault(d => d.Id == idDocJournal);
            return docJournal;
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

            if(oldIdDocJournal != null)
                labels = _abt.DocGoodsDetailsLabels.Where(l => l.IdDoc == oldIdDocJournal && markedCodes.Any(m => m == l.DmLabel));
            else
                labels = _abt.DocGoodsDetailsLabels.Where(l => markedCodes.Any(m => m == l.DmLabel));

            if (labels.Count() == 0)
                return new List<string>();

            foreach (var label in labels)
                label.IdDoc = idDocJournal;

            return labels.Select(l => l.DmLabel);
        }

        public object AddDocumentToDataBase(Models.ConsignorOrganization myOrganization, IEdoSystemDocument<string> document, byte[] content, WebSystems.DocumentInOutType inOutType = WebSystems.DocumentInOutType.None)
        {
            var doc = document as DiadocEdoDocument;
            string orgInn = myOrganization.OrgInn, orgKpp = myOrganization.OrgKpp, orgName = myOrganization.OrgName;

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
                newDoc.ReceiverInn = orgInn;
                newDoc.ReceiverKpp = orgKpp;
                newDoc.ReceiverName = orgName;
            }
            else if (inOutType == WebSystems.DocumentInOutType.Outbox)
            {
                newDoc.SenderInn = orgInn;
                newDoc.SenderKpp = orgKpp;
                newDoc.SenderName = orgName;
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

            var docProcessing = (from d in _abt.DocEdoProcessings
                                 where d.MessageId == newDoc.IdDocEdo && d.EntityId == newDoc.ParentEntityId
                                 select d)?.FirstOrDefault();

            if (docProcessing != null)
                newDoc.IdDocJournal = docProcessing.IdDoc;

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

        public object AddDocEdoReturnPurchasing(decimal idDocJournal, string messageId, string entityId, string sellerFileName, string buyerFileName,
            string senderInn, string senderName, string receiverInn, string receiverName, DateTime docDate, int docStatus = (int)WebSystems.DocEdoStatus.Sent)
        {
            var newReturnDoc = new DocEdoReturnPurchasing
            {
                Id = Guid.NewGuid().ToString(),
                IdDocJournal = idDocJournal,
                MessageId = messageId,
                EntityId = entityId,
                SellerFileName = sellerFileName,
                BuyerFileName = buyerFileName,
                UserName = _dataBaseUser,
                SenderInn = senderInn,
                SenderName = senderName,
                ReceiverInn = receiverInn,
                ReceiverName = receiverName,
                DocDate = docDate,
                DocStatus = docStatus
            };

            _abt.DocEdoReturnPurchasings.Add(newReturnDoc);

            if (!_abt.Entry(newReturnDoc).Reference("Status").IsLoaded)
                _abt.Entry(newReturnDoc).Reference("Status").Load();

            return newReturnDoc;
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

        public void LoadStatus(DocEdoReturnPurchasing doc)
        {
            doc.Status = null;
            _abt.Entry(doc).Reference("Status").IsLoaded = false;
            _abt.Entry(doc).Reference("Status").Load();
        }

        public void UpdateMarkedCodeIncomingStatuses(decimal idDocJournal, WebSystems.MarkedCodeComingStatus status)
        {
            var docJournal = _abt.DocJournals.First(d => d.Id == idDocJournal);

            if(docJournal.IdDocType == (int)DataContextManagementUnit.DataAccess.DocJournalType.Receipt)
                _abt.Database.ExecuteSqlCommand($"UPDATE doc_goods_details_labels SET LABEL_STATUS = {(int)status}, POST_DATETIME = sysdate where id_doc = {idDocJournal}");
        }

        public void UpdateRefGoodForMarkedCodes(decimal idDocJournal, decimal oldIdGood, decimal newIdGood)
        {
            var labels = _abt.DocGoodsDetailsLabels.Where(l => l.IdDoc == idDocJournal && l.IdGood == oldIdGood);

            foreach (var label in labels)
                label.IdGood = newIdGood;
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

        public string GetBarCodeByIdGood(decimal idGood)
        {
            var barCodes = from refBarCode in _abt.RefBarCodes
                           where refBarCode.IdGood == idGood && refBarCode.IsPrimary == false && refBarCode.BarCode != null
                           select refBarCode.BarCode;

            return barCodes.FirstOrDefault();
        }

        public IEnumerable<object> GetRefBarCodesByBarCodes(IEnumerable<string> barCodes)
        {
            if (barCodes == null || barCodes.Count() == 0)
                return new List<RefBarCode>();

            return _abt.RefBarCodes.Where(r => barCodes.Any(b => b == r.BarCode));
        }

        public List<object> GetAllRefGoods()
        {
            return _abt.RefGoods.ToList<object>();
        }

        public object GetRefGoodById(decimal idGood)
        {
            var refGood = (from r in _abt.RefGoods
                           where r.Id == idGood
                           select r).FirstOrDefault();

            return refGood;
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

        public IEnumerable<string> GetMarkedCodesByDocGoodId(object docJournalObj, decimal? idGood)
        {
            var docJournal = docJournalObj as DocJournal;

            if (docJournal == null)
                return null;

            var idDoc = docJournal.Id;

            if (docJournal.IdDocType == (int)DataContextManagementUnit.DataAccess.DocJournalType.Translocation)
                return from label in _abt.DocGoodsDetailsLabels
                       where label.IdDocSale == idDoc && label.IdGood == idGood
                       select label.DmLabel;
            else if (docJournal.IdDocType == (int)DataContextManagementUnit.DataAccess.DocJournalType.Receipt)
                return from label in _abt.DocGoodsDetailsLabels
                       where label.IdDoc == idDoc && label.IdGood == idGood
                       select label.DmLabel;
            else
                return null;
        }

        public object GetCustomerByOrgInn(string inn, string kpp = null)
        {
            var custs = from c in _abt.RefCustomers
                        where c.Inn == inn
                        let isKppNull = kpp == null
                        where isKppNull || c.Kpp == kpp
                        select c;
            return custs.FirstOrDefault();
        }

        public object GetDocEdoReturnPurchasing(decimal idDocJournal)
        {
            return from r in _abt.DocEdoReturnPurchasings
                   where r.IdDocJournal == idDocJournal
                   select r;
        }

        public Dictionary<string, List<KeyValuePair<string, IEnumerable<object>>>> GetMarkedCodesByConsignors(decimal idDocReturn)
        {
            var idDocSaleCollection = (from label in _abt.DocGoodsDetailsLabels
                                       where label.IdDocReturn == idDocReturn && label.IdDocSale != null
                                       select label.IdDocSale.Value).Distinct();

            if (idDocSaleCollection.Count() == 0)
                throw new Exception("Не найдены документы отгрузки.");

            var docComissionEdoProcessings = from docComissionEdoProcessing in _abt.DocComissionEdoProcessings
                                             join docEdoProcessing in _abt.DocEdoProcessings
                                             on docComissionEdoProcessing.Id equals (docEdoProcessing.IdComissionDocument)
                                             join docEdoPurchasing in _abt.DocEdoPurchasings
                                             on docEdoProcessing.MessageId equals (docEdoPurchasing.IdDocEdo)
                                             where docEdoPurchasing.IdDocJournal == docEdoProcessing.IdDoc
                                             join idDocSale in idDocSaleCollection on docEdoPurchasing.IdDocJournal equals (idDocSale)
                                             where docComissionEdoProcessing != null && docEdoPurchasing != null
                                             let labels = from label in _abt.DocGoodsDetailsLabels
                                                          where label.IdDocReturn == idDocReturn && label.IdDocSale == idDocSale
                                                          select label
                                             select new { DocComissionEdoProcessing = docComissionEdoProcessing,
                                                 DocEdoPurchasing = docEdoPurchasing,
                                                 Labels = labels
                                             };

            var docsCount = docComissionEdoProcessings.Count();

            if (idDocSaleCollection.Count() > docsCount)
                throw new Exception("Не все документы отгрузки были отправлены.");

            var idDocSaleList = idDocSaleCollection.ToList();
            if (idDocSaleList.Exists(i => docComissionEdoProcessings
            .FirstOrDefault(d => d.DocComissionEdoProcessing.IdDoc == i && d.DocComissionEdoProcessing.DocStatus == (int)WebSystems.DocEdoStatus.Processed) == null))
                throw new Exception("Не все документы отгрузки были обработаны при комиссионной отправке.");

            if (idDocSaleList.Exists(i => docComissionEdoProcessings
            .FirstOrDefault(d => d.DocEdoPurchasing.IdDocJournal == i && d.DocEdoPurchasing.DocStatus == (int)WebSystems.DocEdoStatus.Processed) == null))
                throw new Exception("Не все документы отгрузки были оприходованы.");

            var resultCollection = new Dictionary<string, List<KeyValuePair<string, IEnumerable<object>>>>();
            var groupsBySender = docComissionEdoProcessings.GroupBy(d => d.DocComissionEdoProcessing.SenderInn);

            foreach (var groupBySender in groupsBySender)
            {
                var groupsByReceiver = groupBySender.GroupBy(d => d.DocEdoPurchasing.ReceiverInn);
                var list = new List<KeyValuePair<string, IEnumerable<object>>>();
                foreach (var groupByReceiver in groupsByReceiver)
                {
                    list.Add(new KeyValuePair<string, IEnumerable<object>>(groupByReceiver.Key, groupByReceiver.SelectMany(v => v.Labels)));
                }

                resultCollection.Add(groupBySender.Key, list);
            }

            return resultCollection;
        }

        public IEnumerable<KeyValuePair<TKey, TValue>> GetMyOrganisations<TKey, TValue>(string userName)
        {
            IEnumerable<KeyValuePair<TKey, TValue>> orgs = (from myOrg in _abt.RefUsersByEdoConsignors
                       where myOrg.UserName == userName
                       join refCustomerConsignor in _abt.RefCustomers
                       on myOrg.IdCustomerConsignor equals (refCustomerConsignor.Id)
                       join refCustomerShipper in _abt.RefCustomers
                       on myOrg.IdCustomerShipper equals (refCustomerShipper.Id)
                       select new { RefCustomerConsignor = refCustomerConsignor, RefCustomerShipper = refCustomerShipper })?
                       .GroupBy(r => r.RefCustomerConsignor)?.ToList()?
                       .Select(s => new KeyValuePair<RefCustomer, IEnumerable<RefCustomer>>(s.Key, s.Select(l => l.RefCustomerShipper)))?
                       .Cast<KeyValuePair<TKey, TValue>>() ?? new List<KeyValuePair<TKey, TValue>>();

            return orgs;
        }

        public List<object> GetHonestMarkProductGroups()
        {
            return _abt.RefHonestMarkProductGroups.ToList<object>();
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
                if (detail?.Quantity == null || detail?.Quantity == 0)
                    continue;

                if (detail.Subtotal != null)
                {
                    decimal? detailPrice = detail.Subtotal / detail.Quantity;
                    parameters = new object[]
                    {
                        new Oracle.ManagedDataAccess.Client.OracleParameter("p_id_doc", idDoc),
                        new Oracle.ManagedDataAccess.Client.OracleParameter("p_id_good", detail.IdGood),
                        new Oracle.ManagedDataAccess.Client.OracleParameter("p_quantity", detail.Quantity),
                        new Oracle.ManagedDataAccess.Client.OracleParameter("p_price", detailPrice)
                    };
                }
                else
                {
                    parameters = new object[]
                    {
                        new Oracle.ManagedDataAccess.Client.OracleParameter("p_id_doc", idDoc),
                        new Oracle.ManagedDataAccess.Client.OracleParameter("p_id_good", detail.IdGood),
                        new Oracle.ManagedDataAccess.Client.OracleParameter("p_quantity", detail.Quantity)
                    };
                }

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

        public bool IsExistsNotReceivedCodes(decimal idDoc, int docType)
        {
            int count = 0;

            if(docType == (int)DataContextManagementUnit.DataAccess.DocJournalType.Receipt)
                count = _abt.Database.SqlQuery<int>($"select count(*) from doc_goods_details_labels where id_doc = {idDoc} and LABEL_STATUS <> 1").First();
            else if(docType == (int)DataContextManagementUnit.DataAccess.DocJournalType.Translocation)
                count = _abt.Database.SqlQuery<int>($"select count(*) from doc_goods_details_labels where id_doc_sale = {idDoc} and LABEL_STATUS <> 2").First();

            return count > 0;
        }

        public List<string> GetErrorsWithMarkedCodes(decimal idDoc, int docType)
        {
            if (docType == (int)DataContextManagementUnit.DataAccess.DocJournalType.Translocation)
            {
                var errors = _abt.Database.SqlQuery<string>($"select DECODE(d.label_status, 1, 'Код маркировки '|| d.dm_label || ' не был оприходован', " +
                $"DECODE(d.label_status, 0, 'Код маркировки ' || d.dm_label || ' не был пропикан', '') ) " +
                $"from doc_goods_details_labels d where d.label_status <> 2 and length(d.dm_label) = 31 and d.id_doc_sale = {idDoc}");
                return errors.ToList();
            }
            else
            {
                var errors = _abt.Database.SqlQuery<string>($"select DECODE(d.label_status, 2, 'Код маркировки '|| d.dm_label || ' уже был оприходован', " +
                $"DECODE(d.label_status, 0, 'Код маркировки ' || d.dm_label || ' не был пропикан', '') ) " +
                $"from doc_goods_details_labels d where d.label_status <> 1 and length(d.dm_label) = 31 and d.id_doc = {idDoc}");
                return errors.ToList();
            }
        }

        public System.Data.Entity.DbContextTransaction BeginTransaction()
        {
            return _abt.Database.BeginTransaction();
        }

        public void ReloadEntry(object entry)
        {
            if(_abt.Entry(entry)?.State != System.Data.Entity.EntityState.Added)
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
