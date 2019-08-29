using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace OpenSearch.Data
{
    [XmlRoot("OpenSearchDescription", Namespace = "http://a9.com/-/spec/opensearch/1.1/")]
    public class OpenSearchDescription
    {
        [XmlNamespaceDeclarations]
        public XmlSerializerNamespaces xmlns
        {
            get
            {
                var ns = new XmlSerializerNamespaces();
                ns.Add(string.Empty, string.Empty);
                return ns;
            }
        }

        [XmlElement("ShortName", Namespace = "http://a9.com/-/spec/opensearch/1.1/")]
        public string ShortName { get; set; }

        [XmlElement("Description", Namespace = "http://a9.com/-/spec/opensearch/1.1/")]
        public string Description { get; set; }

        [XmlElement("InputEncoding", Namespace = "http://a9.com/-/spec/opensearch/1.1/")]
        public string InputEncoding { get; set; }

        [XmlElement("Url", Namespace = "http://a9.com/-/spec/opensearch/1.1/")]
        public List<Url> Urls { get; set; }

        [XmlElement("Image", Namespace = "http://a9.com/-/spec/opensearch/1.1/")]
        public Image Image { get; set; }

        [XmlElement("SearchForm")]
        public string SearchForm { get; set; }

        [XmlElement("Contact", Namespace = "http://a9.com/-/spec/opensearch/1.1/")]
        public string Contact { get; set; }
    }
}
