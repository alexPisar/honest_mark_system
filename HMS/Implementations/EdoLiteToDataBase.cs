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
            });

            _abt.DocGoodsDetailsLabels.AddRange(labels);
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
