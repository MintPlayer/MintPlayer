using System.Xml.Serialization;

namespace OpenSearch.Data
{
    public class Url
    {
        [XmlAttribute("type")]
        public string Type { get; set; }

        [XmlAttribute("method")]
        public string Method { get; set; }

        [XmlAttribute("rel")]
        public string Relation { get; set; }

        [XmlAttribute("template")]
        public string Template { get; set; }
    }
}
