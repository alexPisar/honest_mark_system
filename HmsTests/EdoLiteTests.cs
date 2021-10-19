using System;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Cryptography.WinApi;
using WebSystems.WebClients;
using System.Xml;

namespace HmsTests
{
    [TestClass]
    public class EdoLiteTests
    {
        [TestMethod]
        public void EdoLiteAuthorizationTest()
        {
            var crypto = new WinApiCryptWrapper();
            var cert = crypto.GetCertificateWithPrivateKey("F88D4A47F8C9E5783535D50D4E20F1B0FB421892", false);
            var edo = EdoLiteClient.GetInstance();
            edo.Authorization(cert);
            var hmsClient = HonestMarkClient.GetInstance();
            hmsClient.Authorization(cert);
        }

        [TestMethod]
        public void GetDocumentsTest()
        {
            var crypto = new WinApiCryptWrapper();
            var cert = crypto.GetCertificateWithPrivateKey("F88D4A47F8C9E5783535D50D4E20F1B0FB421892", false);
            var edo = EdoLiteClient.GetInstance();
            edo.Authorization(cert);

            var documentList = edo.GetIncomingDocumentList(6);//, new DateTime(2021, 10, 4));
        }

        [TestMethod]
        public void GetDocumentContentTest()
        {
            var crypto = new WinApiCryptWrapper();
            var cert = crypto.GetCertificateWithPrivateKey("F88D4A47F8C9E5783535D50D4E20F1B0FB421892", false);
            var edo = EdoLiteClient.GetInstance();
            edo.Authorization(cert);

            var documentId = "1289f34d-a6d6-449e-a8d8-38ba15518a0c";
            var documentContent = edo.GetIncomingDocumentContent(documentId);

            var xml = Encoding.GetEncoding(1251).GetString(documentContent);

            var xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(xml);

            var docName = xmlDocument.LastChild.Attributes["ИдФайл"].Value;
        }
    }
}
