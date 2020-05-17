using Newtonsoft.Json;
using System.Collections.Generic;
using System.Drawing;
using System.Xml.Serialization;

namespace MintPlayer.Dtos.Dtos
{
    public class TagCategory
    {
        public TagCategory()
        {
        }

        public int Id { get; set; }
        [JsonConverter(typeof(Converters.HtmlColorConverter))]
        public Color Color { get; set; }
        public string Description { get; set; }

        public List<Tag> Tags { get; set; }

        [XmlIgnore]
        public int TotalTagCount { get; internal set; }
    }
}
