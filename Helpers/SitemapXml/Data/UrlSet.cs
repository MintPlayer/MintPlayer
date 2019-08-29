using System.Collections.Generic;
using System.Xml.Serialization;

namespace SitemapXml
{
    [XmlRoot("urlset", Namespace = "http://www.sitemaps.org/schemas/sitemap/0.9")]
    public class UrlSet
    {
        public UrlSet()
        {
            xmlns.Add("xhtml", "http://www.w3.org/1999/xhtml");
        }

        public UrlSet(IEnumerable<Url> urls) : this()
        {
            Urls.AddRange(urls);
        }

        [XmlNamespaceDeclarations]
        public XmlSerializerNamespaces xmlns = new XmlSerializerNamespaces();

        /// <summary>List of URLs</summary>
        [XmlElement("url", Namespace = "http://www.sitemaps.org/schemas/sitemap/0.9")]
        public List<Url> Urls { get; set; } = new List<Url>();
    }
}
