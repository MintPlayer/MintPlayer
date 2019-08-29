using System.Xml.Serialization;

namespace SitemapXml
{
    public class Link
    {
        /// <summary>Relation for this link. eg. "alternate"</summary>
        [XmlAttribute("rel")]
        public string Rel { get; set; }

        /// <summary>URL for this link</summary>
        [XmlAttribute("href")]
        public string Href { get; set; }

        /// <summary>Language for this link</summary>
        [XmlAttribute("hreflang")]
        public string HrefLang { get; set; }
    }
}
