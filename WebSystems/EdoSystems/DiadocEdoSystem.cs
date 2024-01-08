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

        public override string EdoId => "2BM";

        public override string EdoOrgName => "АО \"ПФ \"СКБ Контур\"";

        public override string EdoOrgInn => "6663003127";

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

        public override object SendDocument(string documentId, byte[] content, byte[] signature, string emchdId, params object[] parameters)
        {
            string entityId = (string)parameters[0];
            int docType = (int)parameters[1];

            string boxId = null;
            X509Certificate2 cert = null;
            string orgInn = this.CurrentOrgInn;

            if (parameters.Length > 2)
            {
                boxId = parameters[2] as string;
                cert = parameters[3] as X509Certificate2;
                orgInn = parameters[4] as string;
            }

            var recipientAttachment = new Diadoc.Api.Proto.Events.RecipientTitleAttachment
            {
                ParentEntityId = entityId,
                SignedContent = new Diadoc.Api.Proto.Events.SignedContent
                {
                    Content = content
                }
            };

            if (!string.IsNullOrEmpty(emchdId))
                recipientAttachment.SignedContent.PowerOfAttorney = new Diadoc.Api.Proto.Events.PowerOfAttorneyToPost
                {
                    UseDefault = false,
                    FullId = new Diadoc.Api.Proto.PowersOfAttorney.PowerOfAttorneyFullId
                    {
                        RegistrationNumber = emchdId,
                        IssuerInn = orgInn
                    }
                };

            if (signature != null)
                recipientAttachment.SignedContent.Signature = signature;

            if(string.IsNullOrEmpty(boxId))
                return ((WebClients.DiadocEdoClient)_webClient).SendPatchRecipientXmlDocument(documentId, docType, recipientAttachment);
            else
                return ((WebClients.DiadocEdoClient)_webClient).SendPatchRecipientXmlDocument(documentId, docType, recipientAttachment, boxId, cert);
        }

        public override object SendUniversalTransferDocument(byte[] content, byte[] signature, string emchdId, params object[] parameters)
        {
            var myOrgId = parameters[0] as string;
            var recipientInn = parameters[1] as string;
            var function = parameters[2] as string;
            var comment = parameters[3] as string;
            var customDocumentId = parameters[4] as string;

            var documentAttachment = new Diadoc.Api.Proto.Events.DocumentAttachment
            {
                TypeNamedId = "UniversalTransferDocument",
                Function = function,
                Version = "utd820_05_01_01_hyphen",
                SignedContent = new Diadoc.Api.Proto.Events.SignedContent
                {
                    Content = content
                }
            };

            if (!string.IsNullOrEmpty(emchdId))
                documentAttachment.SignedContent.PowerOfAttorney = new Diadoc.Api.Proto.Events.PowerOfAttorneyToPost
                {
                    UseDefault = false,
                    FullId = new Diadoc.Api.Proto.PowersOfAttorney.PowerOfAttorneyFullId
                    {
                        RegistrationNumber = emchdId,
                        IssuerInn = this.CurrentOrgInn
                    }
                };

            documentAttachment.Comment = comment;
            documentAttachment.CustomDocumentId = customDocumentId;

            if(signature != null)
                documentAttachment.SignedContent.Signature = signature;

            var counteragentOrgId = GetCounteragentOrgId(myOrgId, recipientInn, null);

            return ((WebClients.DiadocEdoClient)_webClient).SendXmlDocument(myOrgId, counteragentOrgId, false, documentAttachment);
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
                documents = ((WebClients.DiadocEdoClient)_webClient).GetDocuments("Any.InboundNotFinished", fromDate, toDate);
            else if(inOutType == DocumentInOutType.Outbox)
                documents = ((WebClients.DiadocEdoClient)_webClient).GetDocuments("Any.OutboundNotFinished", fromDate, toDate);

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

        public override void SendRevokeConfirmation(byte[] signature, string emchdId, params object[] parameters)
        {
            var messageId = (string)parameters[0];
            var entityId = (string)parameters[1];

            Diadoc.Api.Proto.Events.PowerOfAttorneyToPost powerOfAttorney = null;

            if (!string.IsNullOrEmpty(emchdId))
                powerOfAttorney = new Diadoc.Api.Proto.Events.PowerOfAttorneyToPost
                {
                    UseDefault = false,
                    FullId = new Diadoc.Api.Proto.PowersOfAttorney.PowerOfAttorneyFullId
                    {
                        RegistrationNumber = emchdId,
                        IssuerInn = this.CurrentOrgInn
                    }
                };

            ((WebClients.DiadocEdoClient)_webClient).SendPatchSignedDocument(messageId, entityId, signature, powerOfAttorney);
        }

        public override void SendRejectionDocument(string function, byte[] fileBytes, byte[] signature, string emchdId, params object[] parameters)
        {
            var messageId = (string)parameters[0];
            var entityId = (string)parameters[1];

            Diadoc.Api.Proto.Events.PowerOfAttorneyToPost powerOfAttorney = null;

            if(!string.IsNullOrEmpty(emchdId))
                powerOfAttorney = new Diadoc.Api.Proto.Events.PowerOfAttorneyToPost
                {
                    UseDefault = false,
                    FullId = new Diadoc.Api.Proto.PowersOfAttorney.PowerOfAttorneyFullId
                    {
                        RegistrationNumber = emchdId,
                        IssuerInn = this.CurrentOrgInn
                    }
                };

            if (function == "СЧФ")
                ((WebClients.DiadocEdoClient)_webClient).SendInvoiceCorrectionDocument(messageId, entityId, fileBytes, signature, powerOfAttorney);
            else
                ((WebClients.DiadocEdoClient)_webClient).SendRejectionDocument(messageId, entityId, fileBytes, signature, powerOfAttorney);
        }

        public override void SendRevocationDocument(string function, byte[] fileBytes, byte[] signature, string emchdId, params object[] parameters)
        {
            var messageId = (string)parameters[0];
            var entityId = (string)parameters[1];

            Diadoc.Api.Proto.Events.PowerOfAttorneyToPost powerOfAttorney = null;

            if (!string.IsNullOrEmpty(emchdId))
                powerOfAttorney = new Diadoc.Api.Proto.Events.PowerOfAttorneyToPost
                {
                    UseDefault = false,
                    FullId = new Diadoc.Api.Proto.PowersOfAttorney.PowerOfAttorneyFullId
                    {
                        RegistrationNumber = emchdId,
                        IssuerInn = this.CurrentOrgInn
                    }
                };

            ((WebClients.DiadocEdoClient)_webClient).SendRevocationDocument(messageId, entityId, fileBytes, signature, powerOfAttorney);
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

        public override string GetOrganizationEdoIdByInn(string inn, string myOrgInn, params object[] parameters)
        {
            if (inn == myOrgInn)
            {
                var organization = ((WebClients.DiadocEdoClient)_webClient).GetMyOrganizationByInnKpp(inn);
                return organization.FnsParticipantId;
            }
            else
            {
                var myOrgId = parameters[0] as string;
                var counteragents = ((WebClients.DiadocEdoClient)_webClient).GetKontragents(myOrgId);

                var fnsParticipantId = (from c in counteragents
                                        where c?.Organization?.Inn == inn
                                        select c.Organization.FnsParticipantId).FirstOrDefault();

                return fnsParticipantId;
            }
        }

        public override void SaveParameters(params object[] parameters)
        {
            ((WebClients.DiadocEdoClient)_webClient).SaveEdoLastDateTime((DateTime)parameters[0]);
        }
    }
}
