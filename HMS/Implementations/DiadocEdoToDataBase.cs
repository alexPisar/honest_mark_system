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
        private AbtDbContext _abt;
        private List<Diadoc.Api.Proto.Box> _permittedBoxes;
        private List<DocEdoPurchasing> _documents;
        private bool _isContextInitialized = false;

        public void InitializeContext()
        {
            _abt = new AbtDbContext();

            _dataBaseUser = ConfigSet.Configs.Config.GetInstance().DataBaseUser;

            _documents = _abt.DocEdoPurchasings
                .Where(d => d.EdoProviderName == providerName && d.ReceiverInn == _orgInn)
                .ToList();

            _isContextInitialized = true;
        }

        public void SetPermittedBoxIds(List<KeyValuePair<Diadoc.Api.Proto.Box, Diadoc.Api.Proto.Organization>> boxesByInn)
        {
            if(!_isContextInitialized)
                InitializeContext();

            var permittedSenderInnsForUser = (from refUser in _abt.RefUsersByEdoShippers
                                              join cus in _abt.RefCustomers on refUser.IdCustomer equals cus.Id
                                              where refUser.UserName == _dataBaseUser
                                              select cus.Inn)?.ToList() ?? new List<string>();
            permittedSenderInnsForUser.Add("9652306541");

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

        public void SetOrgData(string orgName, string inn)
        {
            _orgName = orgName;
            _orgInn = inn;
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

        public void AddMarkedCode(object docPurchasing, KeyValuePair<string, string> code)
        {
            if (((DocPurchasing)docPurchasing)?.IdDocLink == null)
                throw new Exception("Для документа закупок не найден трейдер документ.");

            var idDoc = ((DocPurchasing)docPurchasing).IdDocLink.Value;
            var markedCode = code.Key;

            var barCode = code.Value;
            var idGood = _abt.RefBarCodes?
                .FirstOrDefault(b => b.BarCode == barCode && b.IsPrimary == false)?
                .IdGood;

            if (idGood == null)
                throw new Exception($"Товара со штрихкодом {barCode} нет в базе данных.");

            if (_abt.DocGoodsDetailsLabels.FirstOrDefault(l => l.IdDoc == idDoc && l.IdGood == idGood && l.DmLabel == markedCode) != null)
                return;

            var label = new DocGoodsDetailsLabels
            {
                IdDoc = idDoc,
                DmLabel = markedCode,
                IdGood = idGood.Value,
                InsertDateTime = DateTime.Now
            };

            _abt.DocGoodsDetailsLabels.Add(label);
        }

        public object AddDocumentToDataBase(IEdoSystemDocument<string> document, byte[] content, WebSystems.DocumentInOutType inOutType = WebSystems.DocumentInOutType.None)
        {
            var doc = document as DiadocEdoDocument;

            if (doc.DocumentType != Diadoc.Api.Proto.DocumentType.XmlAcceptanceCertificate && doc.DocumentType != Diadoc.Api.Proto.DocumentType.Invoice &&
                doc.DocumentType != Diadoc.Api.Proto.DocumentType.XmlTorg12 && doc.DocumentType != Diadoc.Api.Proto.DocumentType.UniversalTransferDocument)
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

            if (inOutType == WebSystems.DocumentInOutType.Inbox)
            {
                newDoc.SenderInn = counteragentInn;
                newDoc.SenderName = counteragentName;
                newDoc.ReceiverInn = _orgInn;
                newDoc.ReceiverName = _orgName;
            }
            else if (inOutType == WebSystems.DocumentInOutType.Outbox)
            {
                newDoc.SenderInn = _orgInn;
                newDoc.SenderName = _orgName;
                newDoc.ReceiverInn = counteragentInn;
                newDoc.ReceiverName = counteragentName;
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
                    EdoDocument = newDoc
                };

                var refGoods = _abt.RefBarCodes?
                            .Where(b => b.BarCode == newDetail.BarCode && b.IsPrimary == false)?
                            .Select(b => b.IdGood)?.Distinct()?.ToList() ?? new List<decimal?>();

                if (refGoods.Count == 1)
                    newDetail.IdGood = refGoods.First();

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
