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
            {
                entity = currentMessage?.Entities?.FirstOrDefault(c => c.AttachmentType == Diadoc.Api.Proto.Events.AttachmentType.UniversalTransferDocumentRevision);
            }

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
            string entityId = (string)parameters[0];
            int docType = (int)parameters[1];

            var recipientAttachment = new Diadoc.Api.Proto.Events.RecipientTitleAttachment
            {
                ParentEntityId = entityId,
                SignedContent = new Diadoc.Api.Proto.Events.SignedContent
                {
                    Content = content
                }
            };

            if (signature != null)
                recipientAttachment.SignedContent.Signature = signature;

            return ((WebClients.DiadocEdoClient)_webClient).SendPatchRecipientXmlDocument(documentId, docType, recipientAttachment);
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

        public override DocEdoStatus GetCurrentStatus(params object[] parameters)
        {
            var status = (int)parameters[0];
            var messageId = (string)parameters[1];
            var entityId = (string)parameters[2];

            var document = ((WebClients.DiadocEdoClient)_webClient).GetDocument(messageId, entityId);

            DocEdoStatus docStatus = (DocEdoStatus)status;

            if (document.RevocationStatus == Diadoc.Api.Proto.Documents.RevocationStatus.RequestsMyRevocation)
                docStatus = DocEdoStatus.RevokeRequired;
            else if(document.RevocationStatus == Diadoc.Api.Proto.Documents.RevocationStatus.RevocationAccepted)
                docStatus = DocEdoStatus.Revoked;
            else if (document.RevocationStatus == Diadoc.Api.Proto.Documents.RevocationStatus.RevocationRejected)
                docStatus = DocEdoStatus.RejectRevoke;

            return docStatus;
        }

        public override object GetRevokeDocument(out string fileName, out byte[] signature, params object[] parameters)
        {
            var initiatorBoxId = (string)parameters[0];
            var messageId = (string)parameters[1];
            var entityId = (string)parameters[2];

            var currentMessage = ((WebClients.DiadocEdoClient)_webClient).GetMessage(messageId, entityId, true);

            var entity = currentMessage?.Entities?.FirstOrDefault(c => c.AttachmentType == Diadoc.Api.Proto.Events.AttachmentType.RevocationRequest &&
            c.RevocationRequestInfo?.InitiatorBoxId == initiatorBoxId);

            fileName = entity.FileName;
            signature = currentMessage?.Entities?
                .FirstOrDefault(s => s.ParentEntityId == entity.EntityId && s.EntityType == Diadoc.Api.Proto.Events.EntityType.Signature)?
                .Content?.Data;

            return entity;
        }

        public override void SendRevokeConfirmation(byte[] signature, params object[] parameters)
        {
            var messageId = (string)parameters[0];
            var entityId = (string)parameters[1];

            ((WebClients.DiadocEdoClient)_webClient).SendPatchSignedDocument(messageId, entityId, signature);
        }

        public override void SendRejectionDocument(string function, byte[] fileBytes, byte[] signature, params object[] parameters)
        {
            var messageId = (string)parameters[0];
            var entityId = (string)parameters[1];

            if (function == "СЧФ")
                ((WebClients.DiadocEdoClient)_webClient).SendInvoiceCorrectionDocument(messageId, entityId, fileBytes, signature);
            else
                ((WebClients.DiadocEdoClient)_webClient).SendRejectionDocument(messageId, entityId, fileBytes, signature);
        }

        public override void SendRevocationDocument(string function, byte[] fileBytes, byte[] signature, params object[] parameters)
        {
            var messageId = (string)parameters[0];
            var entityId = (string)parameters[1];

            ((WebClients.DiadocEdoClient)_webClient).SendRevocationDocument(messageId, entityId, fileBytes, signature);
        }

        public override byte[] GetDocumentPrintForm(params object[] parameters)
        {
            var messageId = (string)parameters[0];
            var entityId = (string)parameters[1];

            var printForm = ((WebClients.DiadocEdoClient)_webClient).GetPrintForm(messageId, entityId);
            return printForm.Bytes;
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

        public string GetKppForMyOrganization(string inn)
        {
            var organization = ((WebClients.DiadocEdoClient)_webClient).GetMyOrganizationByInnKpp(inn);
            return organization.Kpp;
        }

        public override string GetOrganizationEdoIdByInn(string inn, params object[] parameters)
        {
            var myOrgId = parameters[0] as string;
            var counteragents = ((WebClients.DiadocEdoClient)_webClient).GetKontragents(myOrgId);

            var fnsParticipantId = (from c in counteragents
                    where c?.Organization?.Inn == inn
                    select c.Organization.FnsParticipantId).FirstOrDefault();

            return fnsParticipantId;
        }
    }
}
