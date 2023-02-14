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

        public override string EdoId => "2LT";

        public override string EdoOrgName => "ООО \"ОПЕРАТОР-ЦРПТ\"";

        public override string EdoOrgInn => "7731376812";

        public override bool HasZipContent => true;

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

        public override byte[] GetZipContent(string documentId, DocumentInOutType inOutType = DocumentInOutType.None)
        {
            var webClient = (WebClients.EdoLiteClient)_webClient;
            byte[] zipBytes = null;

            if(inOutType == DocumentInOutType.Inbox)
            {
                zipBytes = webClient.GetIncomingZipDocument(documentId);
            }
            else if (inOutType == DocumentInOutType.Outbox)
            {
                zipBytes = webClient.GetOutgoingZipDocument(documentId);
            }

            return zipBytes;
        }

        public override byte[] GetDocumentPrintForm(params object[] parameters)
        {
            var documentId = (string)parameters[0];
            return ((WebClients.EdoLiteClient)_webClient).GetIncomingDocumentPrintForm(documentId);
        }

        public override bool Authorization()
        {
            if (_certificate == null)
                throw new Exception("Не задан сертификат для авторизации");

            return ((WebClients.EdoLiteClient)_webClient).Authorization(_certificate);
        }

        public override object SendDocument(string documentId, byte[] content, byte[] signature, params object[] parameters)
        {
            string reference = parameters[0] as string;
            var signatureAsBase64 = Convert.ToBase64String(signature);

            return ((WebClients.EdoLiteClient)_webClient).LoadTitleDocument(reference, documentId, signatureAsBase64);
        }

        public override object SendUniversalTransferDocument(byte[] content, byte[] signature, params object[] parameters)
        {
            string reference = parameters[0] as string;
            var signatureAsBase64 = Convert.ToBase64String(signature);
            
            return ((WebClients.EdoLiteClient)_webClient).LoadOutgoingDocument(reference, signatureAsBase64);
        }

        public override string GetOrganizationEdoIdByInn(string inn, string myOrgInn, params object[] parameters)
        {
            var honestMarkClient = parameters[0] as Systems.HonestMarkSystem;
            return honestMarkClient.GetEdoIdByInn(inn);
        }

        public override void SaveParameters(params object[] parameters)
        {
            ((WebClients.EdoLiteClient)_webClient).SaveParameters(parameters);
        }
    }
}
