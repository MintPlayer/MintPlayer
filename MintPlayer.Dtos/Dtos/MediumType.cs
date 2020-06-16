using MintPlayer.Dtos.Enums;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace MintPlayer.Dtos.Dtos
{
	[XmlRoot(Namespace = "https://mintplayer.com/music")]
	[XmlType(Namespace = "https://mintplayer.com/music")]
	[DataContract(Namespace = "https://mintplayer.com/music")]
	public class MediumType
	{
		[DataMember]
		[XmlElement(Namespace = "https://mintplayer.com/music")]
		public int Id { get; set; }

		[DataMember]
		[XmlElement(Namespace = "https://mintplayer.com/music")]
		public string Description { get; set; }
		
		[DataMember]
		[XmlElement(Namespace = "https://mintplayer.com/music")]
		public ePlayerType PlayerType { get; set; }
		
		[DataMember]
		[XmlElement(Namespace = "https://mintplayer.com/music")]
		public bool Visible { get; set; }
	}
}
