using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataContextManagementUnit.DataAccess.Contexts.Abt;
using HonestMarkSystem.Interfaces;
using WebSystems.Models;

namespace HonestMarkSystem.Implementations
{
    public class EdoLiteToDataBase : IEdoDataBaseAdapter<AbtDbContext>
    {
        private const string providerName = "EDO LITE";

        private string _orgName;
        private string _orgInn;
        private AbtDbContext _abt;
        private List<DocEdoPurchasing> _documents;

        public void InitializeContext()
        {
            _abt = new AbtDbContext();
            _documents = _abt.DocEdoPurchasings
                .Where(d => d.EdoProviderName == providerName)
                .ToList();
        }

        public object AddDocumentToDataBase(IEdoSystemDocument<string> document, WebSystems.DocumentInOutType inOutType = WebSystems.DocumentInOutType.None)
        {
            var doc = (EdoLiteDocuments)document;

            var newDocInDb = new DocEdoPurchasing()
            {
                IdDocEdo = doc.EdoId,
                EdoProviderName = providerName,
                CreateDate = doc.CreateDateTime,
                ReceiveDate = doc.Date,
                DocStatus = doc.Status,
                Name = doc.Number,
                TotalPrice = doc.TotalPrice,
                TotalVatAmount = doc.TotalVatAmount,
                IdDocType = doc.DocType
            };

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

        public object[] GetAllDocuments()
        {
            return _documents.ToArray();
        }

        public object[] GetPurchasingDocuments()
        {
            return _abt.DocPurchasings.ToArray();
        }

        public void SaveOrgData(string orgInn, string orgName)
        {
            _orgInn = orgInn;
            _orgName = orgName;
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
