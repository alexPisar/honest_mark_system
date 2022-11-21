using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebSystems.EventArgs;
using System.Security.Cryptography.X509Certificates;

namespace WebSystems
{
    public abstract class IEdoSystem
    {
        protected IWebClient _webClient;
        protected X509Certificate2 _certificate;

        public virtual EventHandler<SendReceivingConfirmationEventArgs> SendReceivingConfirmationEventHandler { get; }
        public abstract List<Models.IEdoSystemDocument<string>> GetDocuments(DocumentInOutType inOutType = DocumentInOutType.None, int docCount = 0, DateTime? fromDate = null, DateTime? toDate = null);
        public abstract byte[] GetDocumentContent(Models.IEdoSystemDocument<string> document, DocumentInOutType inOutType = DocumentInOutType.None);

        public abstract string ProgramVersion { get; }

        public abstract string EdoId { get; }

        public abstract string EdoOrgName { get; }

        public abstract string EdoOrgInn { get; }

        public abstract bool HasZipContent { get; }

        public abstract byte[] GetZipContent(string documentId, DocumentInOutType inOutType = DocumentInOutType.None);

        public abstract string GetOrganizationEdoIdByInn(string inn, params object[] parameters);

        public IEdoSystem(X509Certificate2 certificate)
        {
            _certificate = certificate;
        }

        public virtual byte[] GetDocumentContent(Models.IEdoSystemDocument<string> document, out byte[] signature, DocumentInOutType inOutType = DocumentInOutType.None)
        {
            signature = null;
            return GetDocumentContent(document, inOutType);
        }

        public virtual int GetDocumentsCount()
        {
            return 0;
        }

        public virtual byte[] GetDocumentPrintForm(params object[] parameters)
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

        public virtual DocEdoStatus GetCurrentStatus(params object[] parameters)
        {
            return DocEdoStatus.New;
        }

        public virtual object GetRevokeDocument(out string fileName, out byte[] signature, params object[] parameters){ fileName = string.Empty; signature = null;  return new byte[] { }; }

        public virtual void SendRevokeConfirmation(byte[] signature, params object[] parameters) { }

        public virtual void SendRejectionDocument(string function, byte[] fileBytes, byte[] signature, params object[] parameters) { }

        public virtual void SendRevocationDocument(string function, byte[] fileBytes, byte[] signature, params object[] parameters) { }

        public abstract object SendDocument(string documentId, byte[] content, byte[] signature, params object[] parameters);

        public abstract object SendUniversalTransferDocument(byte[] content, byte[] signature, params object[] parameters);
    }
}
