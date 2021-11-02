using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography.X509Certificates;

namespace WebSystems.EdoSystems
{
    public class EdoLiteSystem : IEdoSystem
    {
        public EdoLiteSystem(X509Certificate2 certificate) : base(certificate)
        {
            _webClient = WebClients.EdoLiteClient.GetInstance();
        }

        public override string ProgramVersion => "EDOLite 1.0";

        public override List<Models.IEdoSystemDocument<string>> GetDocuments(DocumentInOutType inOutType = DocumentInOutType.None, int docCount = 0, DateTime? fromDate = null, DateTime? toDate = null)
        {
            var webClient = (WebClients.EdoLiteClient)_webClient;

            Models.EdoLiteDocumentList documentList = null;

            if(inOutType == DocumentInOutType.Inbox)
                documentList = webClient.GetIncomingDocumentList(docCount, fromDate, toDate);
            else if (inOutType == DocumentInOutType.Outbox)
                documentList = webClient.GetOutgoingDocumentList(docCount, fromDate, toDate);

            return documentList?.Items?.ToList<Models.IEdoSystemDocument<string>>();
        }

        public override int GetDocumentsCount()
        {
            var webClient = (WebClients.EdoLiteClient)_webClient;

            return webClient.GetIncomingDocumentList(0).Count;
        }

        public override byte[] GetDocumentContent(Models.IEdoSystemDocument<string> document, DocumentInOutType inOutType = DocumentInOutType.None)
        {
            var webClient = (WebClients.EdoLiteClient)_webClient;
            var doc = (Models.EdoLiteDocuments)document;

            byte[] fileBytes = null;

            if (inOutType == DocumentInOutType.Inbox)
                fileBytes = webClient.GetIncomingDocumentContent(document.EdoId);
            else if (inOutType == DocumentInOutType.Outbox)
                fileBytes = webClient.GetOutgoingDocumentContent(document.EdoId);

            return fileBytes;
        }

        public override bool Authorization()
        {
            if (_certificate == null)
                throw new Exception("Не задан сертификат для авторизации");

            return ((WebClients.EdoLiteClient)_webClient).Authorization(_certificate);
        }
    }
}
