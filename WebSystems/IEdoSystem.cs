using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography.X509Certificates;

namespace WebSystems
{
    public abstract class IEdoSystem
    {
        protected IWebClient _webClient;
        protected X509Certificate2 _certificate;
        public abstract List<Models.IEdoSystemDocument<string>> GetDocuments(DocumentInOutType inOutType = DocumentInOutType.None, int docCount = 0, DateTime? fromDate = null, DateTime? toDate = null);
        public abstract byte[] GetDocumentContent(Models.IEdoSystemDocument<string> document, DocumentInOutType inOutType = DocumentInOutType.None);

        public abstract string ProgramVersion { get; }

        public IEdoSystem(X509Certificate2 certificate)
        {
            _certificate = certificate;
        }

        public virtual int GetDocumentsCount()
        {
            return 0;
        }

        public virtual byte[] GetDocumentPrintForm(Models.IEdoSystemDocument<string> document)
        {
            return null;
        }

        public virtual bool Authorization()
        {
            return false;
        }

        public string GetCertSubject()
        {
            return _certificate?.Subject;
        }
    }
}
