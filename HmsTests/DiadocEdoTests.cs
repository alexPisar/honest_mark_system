using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Cryptography.WinApi;
using WebSystems.WebClients;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HmsTests
{
    [TestClass]
    public class DiadocEdoTests
    {
        [TestMethod]
        public void AuthorizationTest()
        {
            var crypto = new WinApiCryptWrapper();
            var cert = crypto.GetCertificateWithPrivateKey("F88D4A47F8C9E5783535D50D4E20F1B0FB421892", false);
            var edo = DiadocEdoClient.GetInstance();
            var result = edo.Authenticate(cert);
        }

        [TestMethod]
        public void GetEventsTest()
        {
            var crypto = new WinApiCryptWrapper();
            var cert = crypto.GetCertificateWithPrivateKey("0DE1BF746CC43954D0312518732E84621F6432FF", false);
            var edo = DiadocEdoClient.GetInstance();
            edo.Authenticate(cert);

            var events = edo.GetEvents(ConfigSet.Configs.Config.GetInstance().EdoLastDocDateTime.Value);
            var docs = events.Where(e => e.Document != null).ToList();
        }

        [TestMethod]
        public void GetDocumentsTest()
        {
            var crypto = new WinApiCryptWrapper();
            var cert = crypto.GetCertificateWithPrivateKey("0DE1BF746CC43954D0312518732E84621F6432FF", false);
            var edo = DiadocEdoClient.GetInstance();
            edo.Authenticate(cert);

            var docs = edo.GetDocuments("Any.OutboundFinished", ConfigSet.Configs.Config.GetInstance().EdoLastDocDateTime.Value);
            var doc = docs.Last();

            var selectedDoc = edo.GetDocument(doc.MessageId, doc.EntityId);
            //var documents = edo.GetDocumentsByMessageId(doc.MessageId);
            var currentMessage = edo.GetMessage(doc.MessageId, doc.EntityId);

            var signatureDoc = currentMessage.Entities.FirstOrDefault(e => e.ParentEntityId == doc.EntityId && e.EntityType == Diadoc.Api.Proto.Events.EntityType.Signature);

            foreach (var d in docs)
            {
                var events = edo.GetEvents(d.MessageId, d.EntityId, true);
                var ev = events.FirstOrDefault(e => e.DocumentInfo.DocumentType == Diadoc.Api.Proto.DocumentType.UniversalTransferDocument && e.Docflow.OutboundUniversalTransferDocumentDocflow != null);
                var sign = ev.Docflow.DocumentAttachment;
            }
            //var signContent = edo.GetDocument(doc.MessageId, signatureDoc.EntityId);
        }
    }
}
