using MintPlayer.Dtos.Enums;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace MintPlayer.Dtos.Dtos;

[XmlRoot(Namespace = "https://mintplayer.com/music")]
[XmlType(Namespace = "https://mintplayer.com/music")]
[DataContract(Namespace = "https://mintplayer.com/music")]
public abstract class LoginResult
{
	[DataMember]
	[XmlElement(Namespace = "https://mintplayer.com/music")]
	public ELoginStatus Status { get; set; }

	[DataMember]
	[XmlElement(Namespace = "https://mintplayer.com/music")]
	public User User { get; set; }

	[DataMember]
	[XmlElement(Namespace = "https://mintplayer.com/music")]
	public string Error { get; set; }

	[DataMember]
	[XmlElement(Namespace = "https://mintplayer.com/music")]
	public string ErrorDescription { get; set; }
}
