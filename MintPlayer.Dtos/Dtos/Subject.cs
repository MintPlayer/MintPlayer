using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using MintPlayer.Timestamps;
using Newtonsoft.Json;

[assembly: InternalsVisibleTo("MintPlayer.Data")]
namespace MintPlayer.Dtos.Dtos;

[DataContract(Namespace = "https://mintplayer.com/music")]
public abstract class Subject : IUpdateTimestamp
{
	[DataMember]
	[XmlElement(Namespace = "https://mintplayer.com/music")]
	public int Id { get; set; }

	[DataMember]
	[XmlElement(Namespace = "https://mintplayer.com/music")]
	public List<Medium> Media { get; set; }

	[DataMember]
	[XmlElement(Namespace = "https://mintplayer.com/music")]
	public List<Tag> Tags { get; set; }

	[DataMember]
	[XmlElement(Namespace = "https://mintplayer.com/music")]
	public string Text { get; internal set; }

	[DataMember]
	[XmlElement(Namespace = "https://mintplayer.com/music")]
	public string ConcurrencyStamp { get; set; }

	#region DateUpdate
	[XmlIgnore]
	[JsonIgnore]
	[Nest.Ignore]
	[IgnoreDataMember]
	public DateTime DateUpdate { get; set; }

	[XmlIgnore]
	[IgnoreDataMember]
	[JsonProperty("dateUpdate")]
	private DateTime? DateUpdateJson
	{
		get => DateUpdate;
		set
		{
			if (value != null)
				DateUpdate = value.Value;
		}
	}
	#endregion
}
