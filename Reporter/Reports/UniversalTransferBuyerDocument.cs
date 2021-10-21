using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UtilitesLibrary.Service;

namespace Reporter.Reports
{
    public class UniversalTransferBuyerDocument : IReport
    {
        public override void Parse(string content)
        {
            var xsdDocument = Xml.DeserializeEntity<OnNschfdoppokFile>(content);
        }

        public override void Parse(byte[] content)
        {
            var xmlString = Encoding.GetEncoding(1251).GetString(content);
            Parse(xmlString);
        }
    }
}
