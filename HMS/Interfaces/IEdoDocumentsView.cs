using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebSystems.Models;

namespace HonestMarkSystem.Interfaces
{
    public interface IEdoDocumentsView
    {
        List<IEdoSystemDocument<string>> GetNewDocuments(out object[] parameters);

        void SaveParameters(params object[] parameters);

        bool SaveNewDocument(IEdoSystemDocument<string> document, out byte[] fileBytes);
    }
}
