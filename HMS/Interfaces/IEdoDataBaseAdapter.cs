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

        object[] GetPurchasingDocuments();

        object GetPurchasingDocumentById(decimal idDocPurchasing);

        void AddMarkedCode(decimal idDocJournal, decimal idGood, string markedCode);

        void AddMarkedCodes(decimal idDocJournal, decimal idGood, IEnumerable<string> markedCodes);

        void AddMarkedCodes(decimal idDocJournal, List<KeyValuePair<decimal, List<string>>> markedCodesByGoods);

        List<object> GetRefGoodsByBarCode(string barCode);

        List<object> GetAllRefGoods();

        List<object> GetAllMarkedCodes();

        decimal ExportDocument(object documentObject);

        void Rollback();

        void Commit();
    }
}
