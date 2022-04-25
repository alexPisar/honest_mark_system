using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using WebSystems.Models;

namespace HonestMarkSystem.Interfaces
{
    public interface IEdoDataBaseAdapter<TContext> where TContext : DbContext
    {
        void InitializeContext();

        object AddDocumentToDataBase(IEdoSystemDocument<string> document, byte[] content, WebSystems.DocumentInOutType inOutType = WebSystems.DocumentInOutType.None);

        bool ExistsDocumentInDataBase(IEdoSystemDocument<string> document);

        bool DocumentCanBeAddedByUser(IEdoSystemDocument<string> document);

        object GetDocumentFromDb(IEdoSystemDocument<string> document);

        object[] GetAllDocuments(DateTime dateFrom, DateTime dateTo);

        System.Collections.IEnumerable GetJournalDocuments(object selectedDocument);

        object GetPurchasingDocumentById(decimal idDocPurchasing);

        void AddMarkedCode(decimal idDocJournal, decimal idGood, string markedCode);

        void AddMarkedCodes(decimal idDocJournal, decimal idGood, IEnumerable<string> markedCodes);

        void AddMarkedCodes(decimal idDocJournal, List<KeyValuePair<decimal, List<string>>> markedCodesByGoods, IEnumerable<string> updatedCodes = null);

        IEnumerable<string> UpdateCodes(decimal idDocJournal, IEnumerable<string> markedCodes, decimal? oldIdDocJournal = null);

        void UpdateMarkedCodeIncomingStatuses(decimal idDocJournal, WebSystems.MarkedCodeComingStatus status);

        List<object> GetRefGoodsByBarCode(string barCode);

        List<object> GetAllRefGoods();

        List<object> GetAllMarkedCodes();

        IEnumerable<string> GetMarkedCodesByDocumentId(decimal? docJournalId);

        decimal ExportDocument(object documentObject);

        bool IsExistsNotReceivedCodes(decimal idDoc);

        System.Data.Entity.DbContextTransaction BeginTransaction();

        void ReloadEntry(object entry);

        void Commit(System.Data.Entity.DbContextTransaction transaction = null);
        void Dispose();
    }
}
