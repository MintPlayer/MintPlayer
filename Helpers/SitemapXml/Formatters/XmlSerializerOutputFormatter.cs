using System;
using System.IO;
using System.Xml;

namespace SitemapXml.Formatters
{
    public class XmlSerializerOutputFormatter : Microsoft.AspNetCore.Mvc.Formatters.XmlSerializerOutputFormatter
    {
        private string stylesheetUrl;
        public XmlSerializerOutputFormatter(string stylesheetUrl)
        {
            this.stylesheetUrl = stylesheetUrl;
            this.WriterSettings.OmitXmlDeclaration = false;
            this.SupportedMediaTypes.Clear(); this.SupportedMediaTypes.Add("application/xml");
        }

        public override XmlWriter CreateXmlWriter(TextWriter writer, XmlWriterSettings xmlWriterSettings)
        {
            if (writer == null)
            {
                throw new ArgumentNullException(nameof(writer));
            }

            if (xmlWriterSettings == null)
            {
                throw new ArgumentNullException(nameof(xmlWriterSettings));
            }

            // We always close the TextWriter, so the XmlWriter shouldn't.
            xmlWriterSettings.CloseOutput = false;
            
            //var contenttypes = GetSupportedContentTypes(xmlWriterSettings.)

            var xmlWriter = XmlWriter.Create(writer, xmlWriterSettings);
            xmlWriter.WriteProcessingInstruction("xml-stylesheet", $@"type=""text/xsl"" href=""{stylesheetUrl}""");
            return xmlWriter;
        }
    }
}
