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

        object AddDocumentToDataBase(Models.ConsignorOrganization myOrganization, IEdoSystemDocument<string> document, byte[] content, WebSystems.DocumentInOutType inOutType = WebSystems.DocumentInOutType.None);

        object AddDocEdoReturnPurchasing(decimal idDocJournal, string messageId, string entityId, string sellerFileName, string buyerFileName, 
            string senderInn, string senderName, string receiverInn, string receiverName, DateTime docDate, int docStatus = (int)WebSystems.DocEdoStatus.Sent);

        bool ExistsDocumentInDataBase(IEdoSystemDocument<string> document);

        bool DocumentCanBeAddedByUser(Models.ConsignorOrganization myOrganization, IEdoSystemDocument<string> document);

        object GetDocumentFromDb(IEdoSystemDocument<string> document);

        object[] GetAllDocuments(DateTime dateFrom, DateTime dateTo);

        System.Collections.IEnumerable GetJournalDocuments(object selectedDocument);

        System.Collections.IEnumerable GetJournalMarkedDocumentsByType(int docType);

        object GetPurchasingDocumentById(decimal idDocPurchasing);

        object GetDocJournal(decimal idDocJournal);

        void AddMarkedCode(decimal idDocJournal, decimal idGood, string markedCode);

        void AddMarkedCodes(decimal idDocJournal, decimal idGood, IEnumerable<string> markedCodes);

        void AddMarkedCodes(decimal idDocJournal, List<KeyValuePair<decimal, List<string>>> markedCodesByGoods, IEnumerable<string> updatedCodes = null);

        IEnumerable<string> UpdateCodes(decimal idDocJournal, IEnumerable<string> markedCodes, decimal? oldIdDocJournal = null);

        void UpdateMarkedCodeIncomingStatuses(decimal idDocJournal, WebSystems.MarkedCodeComingStatus status);

        void UpdateRefGoodForMarkedCodes(decimal idDocJournal, decimal oldIdGood, decimal newIdGood);

        List<object> GetRefGoodsByBarCode(string barCode);

        string GetBarCodeByIdGood(decimal idGood);

        IEnumerable<object> GetRefBarCodesByBarCodes(IEnumerable<string> barCodes);

        List<object> GetAllRefGoods();

        object GetRefGoodById(decimal idGood);

        List<object> GetAllMarkedCodes();

        IEnumerable<string> GetMarkedCodesByDocumentId(decimal? docJournalId);

        IEnumerable<string> GetMarkedCodesByDocGoodId(object docJournalObj, decimal? idGood);

        object GetCustomerByOrgInn(string inn, string kpp = null);

        object GetDocEdoReturnPurchasing(decimal idDocJournal);

        Dictionary<string, List<KeyValuePair<string, IEnumerable<object>>>> GetMarkedCodesByConsignors(decimal idDocReturn);

        IEnumerable<KeyValuePair<TKey, TValue>> GetMyOrganisations<TKey, TValue>(string userName);

        List<object> GetHonestMarkProductGroups();

        object GetRefAuthoritySignDocumentsByCustomer(decimal idCustomer);

        decimal ExportDocument(object documentObject);

        bool IsExistsNotReceivedCodes(decimal idDoc, int docType);

        List<string> GetErrorsWithMarkedCodes(decimal idDoc, int docType);

        System.Data.Entity.DbContextTransaction BeginTransaction();

        void ReloadEntry(object entry);

        void Commit(System.Data.Entity.DbContextTransaction transaction = null);
        void Dispose();
    }
}
