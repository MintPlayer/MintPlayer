using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace MintPlayer.Dtos.Dtos;

[XmlRoot(Namespace = "https://mintplayer.com/music")]
[XmlType(Namespace = "https://mintplayer.com/music")]
[DataContract(Namespace = "https://mintplayer.com/music")]
public class SearchRequest
{
	[DataMember]
	[XmlElement(Namespace = "https://mintplayer.com/music")]
	public string SearchTerm { get; set; }

	[DataMember]
	[XmlElement(Namespace = "https://mintplayer.com/music")]
	public string[] SubjectTypes { get; set; }
}
