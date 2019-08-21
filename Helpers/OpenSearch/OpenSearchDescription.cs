using System;
using System.Xml.Serialization;

namespace OpenSearch
{
    [XmlRoot("OpenSearchDescription", Namespace = "http://a9.com/-/spec/opensearch/1.1/")]
    public class OpenSearchDescription
    {
        [XmlElement("ShortName", Namespace = "http://a9.com/-/spec/opensearch/1.1/")]
        public string ShortName { get; set; }

        [XmlElement("Description", Namespace = "http://a9.com/-/spec/opensearch/1.1/")]
        public string Description { get; set; }

        [XmlElement("InputEncoding", Namespace = "http://a9.com/-/spec/opensearch/1.1/")]
        public string InputEncoding { get; set; }
    }
}
