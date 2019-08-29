using System.Collections.Generic;
using System.Xml.Serialization;

namespace SitemapXml
{
    [XmlRoot("sitemapindex", Namespace = "http://www.sitemaps.org/schemas/sitemap/0.9")]
    public class SitemapIndex
    {
        public SitemapIndex()
        {
        }

        public SitemapIndex(IEnumerable<Sitemap> sitemaps) : this()
        {
            Sitemaps.AddRange(sitemaps);
        }

        [XmlNamespaceDeclarations]
        public XmlSerializerNamespaces xmlns = new XmlSerializerNamespaces();

        /// <summary>List of sitemaps</summary>
        [XmlElement("sitemap", Namespace = "http://www.sitemaps.org/schemas/sitemap/0.9")]
        public List<Sitemap> Sitemaps { get; set; } = new List<Sitemap>();
    }
}
