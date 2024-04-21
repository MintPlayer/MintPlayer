using Nest;
using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace MintPlayer.Dtos.Dtos;

[XmlRoot(Namespace = "https://mintplayer.com/music")]
[XmlType(Namespace = "https://mintplayer.com/music")]
[DataContract(Namespace = "https://mintplayer.com/music")]
public class Person : Subject
{
	[DataMember]
	[XmlElement(Namespace = "https://mintplayer.com/music")]
	public string FirstName { get; set; }

	[DataMember]
	[XmlElement(Namespace = "https://mintplayer.com/music")]
	public string LastName { get; set; }

	[DataMember]
	[XmlElement(Namespace = "https://mintplayer.com/music")]
	public DateTime? Born { get; set; }

	[DataMember]
	[XmlElement(Namespace = "https://mintplayer.com/music")]
	public DateTime? Died { get; set; }

	[DataMember]
	[XmlElement(Namespace = "https://mintplayer.com/music")]
	public List<Artist> Artists { get; set; }

	[XmlIgnore]
	[JsonIgnore]
	[IgnoreDataMember]
	public CompletionField NameSuggest => new CompletionField { Input = new[] { $"{FirstName} {LastName}" } };
}
