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
        List<IEdoSystemDocument<string>> GetNewDocuments();

        void SaveNewDocument(IEdoSystemDocument<string> document);
    }
}
