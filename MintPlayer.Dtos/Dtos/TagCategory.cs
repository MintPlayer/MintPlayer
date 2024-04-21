using Newtonsoft.Json;
using System.Drawing;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace MintPlayer.Dtos.Dtos;

[XmlRoot(Namespace = "https://mintplayer.com/music")]
[XmlType(Namespace = "https://mintplayer.com/music")]
[DataContract(Namespace = "https://mintplayer.com/music")]
public class TagCategory
{
	[DataMember]
	[XmlElement(Namespace = "https://mintplayer.com/music")]
	public int Id { get; set; }

	[DataMember]
	[XmlElement(Namespace = "https://mintplayer.com/music")]
	[JsonConverter(typeof(Converters.HtmlColorConverter))]
	public Color Color { get; set; }

	[DataMember]
	[XmlElement(Namespace = "https://mintplayer.com/music")]
	public string Description { get; set; }

	[DataMember]
	[XmlElement(Namespace = "https://mintplayer.com/music")]
	public List<Tag> Tags { get; set; }

	[XmlIgnore]
	[IgnoreDataMember]
	public int TotalTagCount { get; internal set; }
}
