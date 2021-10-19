using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebSystems
{
    public abstract class IEdoSystem
    {
        protected IWebClient _webClient;
        public abstract List<Models.IEdoSystemDocument<string>> GetDocuments(DocumentInOutType inOutType = DocumentInOutType.None, int docCount = 0, DateTime? fromDate = null, DateTime? toDate = null);
        public abstract byte[] GetDocumentContent(Models.IEdoSystemDocument<string> document, DocumentInOutType inOutType = DocumentInOutType.None);

        public virtual int GetDocumentsCount()
        {
            return 0;
        }

        public virtual byte[] GetDocumentPrintForm(Models.IEdoSystemDocument<string> document)
        {
            return null;
        }
    }
}
