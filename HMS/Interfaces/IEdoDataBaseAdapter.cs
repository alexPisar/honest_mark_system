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

        object AddDocumentToDataBase(IEdoSystemDocument<string> document);

        bool ExistsDocumentInDataBase(IEdoSystemDocument<string> document);

        object GetDocumentFromDb(IEdoSystemDocument<string> document);

        object[] GetAllDocuments();

        void Rollback();

        void Commit();
    }
}
