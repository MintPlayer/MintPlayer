using System.Xml.Serialization;

namespace OpenSearch.Data
{
    public class Image
    {
        [XmlText]
        public string Url { get; set; }

        [XmlAttribute("width")]
        public int Width { get; set; }

        [XmlAttribute("height")]
        public int Height { get; set; }

        [XmlAttribute("type")]
        public string Type { get; set; }
    }
}
