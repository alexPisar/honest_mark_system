using System;
using System.Xml;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Reporter;
using Reporter.Reports;

namespace HmsTests
{
    [TestClass]
    public class ReporterTests
    {
        [TestMethod]
        public void ParseXmlDocumentTest()
        {
            var reporter = new ReporterDll();

            var xmlDocument = new XmlDocument();
            xmlDocument.Load("C:\\Users\\systech\\Desktop\\Files\\ON_NSCHFDOPPOKMARK_2BM-5032262632-503201001-201601270943558790381_2LT-11000533130_20210924_1967fe1a-ba18-4977-9c7e-0861ddd44298.xml");

            var report = reporter.ParseDocument<UniversalTransferBuyerDocument>(xmlDocument.OuterXml);
        }
    }
}
