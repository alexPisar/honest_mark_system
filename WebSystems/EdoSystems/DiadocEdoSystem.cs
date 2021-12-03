using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebSystems.EventArgs;
using System.Security.Cryptography.X509Certificates;

namespace WebSystems.EdoSystems
{
    public class DiadocEdoSystem : IEdoSystem
    {
        public DiadocEdoSystem(X509Certificate2 certificate) : base(certificate)
        {
            _webClient = WebClients.DiadocEdoClient.GetInstance();
        }

        public override string ProgramVersion => "Diadoc 1.0";

        public override bool HasZipContent => false;

        public override EventHandler<SendReceivingConfirmationEventArgs> SendReceivingConfirmationEventHandler => (object sender, SendReceivingConfirmationEventArgs e) => 
        {
            var doc = (Models.DiadocEdoDocument)e.Document;
            var webClient = (WebClients.DiadocEdoClient)_webClient;

            var currentMessage = webClient.GetMessage(doc.Document.MessageId, doc.Document.EntityId);

            var entity = currentMessage?.Entities?.FirstOrDefault(c => c.AttachmentType == Diadoc.Api.Proto.Events.AttachmentType.UniversalTransferDocument
            || c.AttachmentType == Diadoc.Api.Proto.Events.AttachmentType.Invoice);

            if (entity == null)
                return;

            if (currentMessage.Entities.Exists(c => c.AttachmentType == Diadoc.Api.Proto.Events.AttachmentType.InvoiceReceipt))
                return;

            webClient.SendReceipt(entity.DocumentInfo.MessageId, entity.EntityId);
        };

        public override bool Authorization()
        {
            if (_certificate == null)
                throw new Exception("Не задан сертификат для авторизации");

            return ((WebClients.DiadocEdoClient)_webClient).Authenticate(_certificate);
        }

        public override object SendDocument(string documentId, byte[] content, byte[] signature, params object[] parameters)
        {
            string senderOrgId = (string)parameters[0];
            string receiverOrgId = (string)parameters[1];

            var documentAttachment = new Diadoc.Api.Proto.Events.DocumentAttachment
            {
                TypeNamedId = (string)parameters[2],
                Function = (string)parameters[3],
                Version = (string)parameters[4],
                SignedContent = new Diadoc.Api.Proto.Events.SignedContent
                {
                    Content = content
                }
            };

            documentAttachment.CustomDocumentId = (string)parameters[5];

            if (signature != null)
                documentAttachment.SignedContent.Signature = signature;

            return ((WebClients.DiadocEdoClient)_webClient).SendXmlDocument(senderOrgId, receiverOrgId, false, documentAttachment);
        }

        public string GetMyOrgId(string inn, string kpp = null)
        {
            var organization = ((WebClients.DiadocEdoClient)_webClient).GetMyOrganizationByInnKpp(inn, kpp);

            return organization?.OrgId;
        }

        public string GetCounteragentOrgId(string myOrgId, string inn, string kpp)
        {
            var counteragents = ((WebClients.DiadocEdoClient)_webClient).GetKontragents(myOrgId);

            Diadoc.Api.Proto.Counteragent counteragent;

            if(string.IsNullOrEmpty(kpp))
                counteragent = counteragents.FirstOrDefault(c => c?.Organization?.Inn == inn);
            else
                counteragent = counteragents.FirstOrDefault(c => c?.Organization?.Inn == inn && c?.Organization?.Kpp == kpp);

            if (counteragent?.Organization == null)
                throw new Exception($"Не найден контрагент с ИНН/КПП: {inn}/{kpp}");

            return counteragent.Organization.OrgId;
        }

        public override List<Models.IEdoSystemDocument<string>> GetDocuments(DocumentInOutType inOutType = DocumentInOutType.None, int docCount = 0, DateTime? fromDate = null, DateTime? toDate = null)
        {
            List<Diadoc.Api.Proto.Documents.Document> documents = null;

            if(inOutType == DocumentInOutType.Inbox)
                documents = ((WebClients.DiadocEdoClient)_webClient).GetDocuments("Any.InboundNotFinished", fromDate.Value, toDate);
            else if(inOutType == DocumentInOutType.Outbox)
                documents = ((WebClients.DiadocEdoClient)_webClient).GetDocuments("Any.OutboundNotFinished", fromDate.Value, toDate);

            return documents?.Select(d => new Models.DiadocEdoDocument() { Document = d })?.ToList<Models.IEdoSystemDocument<string>>();
        }

        public override byte[] GetDocumentContent(Models.IEdoSystemDocument<string> document, DocumentInOutType inOutType = DocumentInOutType.None)
        {
            var doc = document as Models.DiadocEdoDocument;
            var selectedDoc = ((WebClients.DiadocEdoClient)_webClient).GetDocument(doc.EdoId, doc.EntityId);
            return selectedDoc.Content?.Data;
        }

        public override byte[] GetDocumentContent(Models.IEdoSystemDocument<string> document, out byte[] signature, DocumentInOutType inOutType = DocumentInOutType.None)
        {
            var webClient = _webClient as WebClients.DiadocEdoClient;
            var doc = document as Models.DiadocEdoDocument;

            var events = webClient.GetEvents(doc.EdoId, doc.EntityId, true);
            var mainEvent = events.FirstOrDefault(e => e.DocumentInfo.DocumentType == doc.DocumentType && e.Docflow?.DocumentAttachment != null);

            var documentAttachment = mainEvent.Docflow.DocumentAttachment;
            signature = documentAttachment.Signature?.Entity?.Content?.Data;
            return documentAttachment.Attachment?.Entity?.Content?.Data;
        }

        public override byte[] GetZipContent(string documentId, DocumentInOutType inOutType = DocumentInOutType.None)
        {
            return null;
        }

        public List<KeyValuePair<Diadoc.Api.Proto.Box, Diadoc.Api.Proto.Organization>> GetCounteragentsBoxesForOrganization(string inn)
        {
            var organization = ((WebClients.DiadocEdoClient)_webClient).GetMyOrganizationByInnKpp(inn);

            if (organization == null)
                throw new Exception($"Не найдена организация с ИНН {inn}");

            var counteragents = ((WebClients.DiadocEdoClient)_webClient).GetKontragents(organization.OrgId)
                .Where(c => c.Organization != null && !string.IsNullOrEmpty(c.Organization.OrgId) && c.Organization.Boxes?.FirstOrDefault() != null);

            var result = (from c in counteragents select new KeyValuePair<Diadoc.Api.Proto.Box, Diadoc.Api.Proto.Organization>(c?.Organization?.Boxes?.FirstOrDefault(), c?.Organization)).ToList();
            return result;
        }
    }
}
