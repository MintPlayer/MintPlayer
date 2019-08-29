using System.Xml.Serialization;

namespace SitemapXml
{
    public class Link
    {
        [XmlAttribute("rel")]
        public string Rel { get; set; }

        [XmlAttribute("href")]
        public string Href { get; set; }

        [XmlAttribute("hreflang")]
        public string HrefLang { get; set; }
    }
}
