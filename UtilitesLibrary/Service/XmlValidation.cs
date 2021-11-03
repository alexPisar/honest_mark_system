using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Schema;

namespace UtilitesLibrary.Service
{
    public class XmlValidation
    {
        public List<string> Errors { get; set; }
        public List<string> Warnings { get; set; }

        public bool ValidationXmlByXsd(string xmlPath, string xsdUrlName, string xsdTargetNamespace = null)
        {
            Errors = new List<string>();
            Warnings = new List<string>();

            XmlReaderSettings xsdSettings = GetSettingsForValidation(xsdUrlName, xsdTargetNamespace);

            XmlReader books = XmlReader.Create(xmlPath, xsdSettings);

            while (books.Read()) { }

            return Errors.Count == 0;
        }

        public bool ValidationXmlByXsd(System.IO.TextReader xmlStream, string xsdUrlName, string xsdTargetNamespace = null)
        {
            Errors = new List<string>();
            Warnings = new List<string>();

            XmlReaderSettings xsdSettings = GetSettingsForValidation(xsdUrlName, xsdTargetNamespace);

            XmlReader books = XmlReader.Create(xmlStream, xsdSettings);

            while (books.Read()) { }

            return Errors.Count == 0;
        }

        private XmlReaderSettings GetSettingsForValidation(string xsdUrlName, string xsdTargetNamespace)
        {
            XmlReaderSettings xsdSettings = new XmlReaderSettings();
            xsdSettings.Schemas.Add(xsdTargetNamespace, xsdUrlName);
            xsdSettings.ValidationType = ValidationType.Schema;
            xsdSettings.ValidationEventHandler += new ValidationEventHandler(booksSettingsValidationEventHandler);

            return xsdSettings;
        }

        private void booksSettingsValidationEventHandler(object sender, ValidationEventArgs e)
        {
            if (e.Severity == XmlSeverityType.Warning)
            {
                Warnings.Add(e.Message);
            }
            else if (e.Severity == XmlSeverityType.Error)
            {
                Errors.Add("- " + e.Message);
            }
        }
    }
}
