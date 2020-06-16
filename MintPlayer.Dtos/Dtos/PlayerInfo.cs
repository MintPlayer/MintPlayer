using MintPlayer.Dtos.Enums;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace MintPlayer.Dtos.Dtos
{
	[XmlRoot(Namespace = "https://mintplayer.com/music")]
	[XmlType(Namespace = "https://mintplayer.com/music")]
	[DataContract(Namespace = "https://mintplayer.com/music")]
    public class PlayerInfo
    {
		[DataMember]
		[XmlElement(Namespace = "https://mintplayer.com/music")]
        public ePlayerType Type { get; set; }

		[DataMember]
		[XmlElement(Namespace = "https://mintplayer.com/music")]
        public string Id { get; set; }
    }
}
