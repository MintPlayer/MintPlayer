using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using SitemapXml.Enums;

namespace SitemapXml
{
    [XmlRoot("url")]
    public class Url
    {
        [XmlElement("loc")]
        public string Loc { get; set; }

        [XmlElement("lastmod")]
        public DateTime LastMod { get; set; }

        [XmlElement("changefreq")]
        public ChangeFreq ChangeFreq { get; set; }

        [XmlElement("link", Namespace = "http://www.w3.org/1999/xhtml")]
        public List<Link> Links { get; set; } = new List<Link>();
    }
}
