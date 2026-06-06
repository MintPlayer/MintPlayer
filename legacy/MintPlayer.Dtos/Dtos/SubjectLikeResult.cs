using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace MintPlayer.Dtos.Dtos
{
	[XmlRoot(Namespace = "https://mintplayer.com/music")]
	[XmlType(Namespace = "https://mintplayer.com/music")]
	[DataContract(Namespace = "https://mintplayer.com/music")]
	public class SubjectLikeResult
	{
		[DataMember]
		[XmlElement(Namespace = "https://mintplayer.com/music")]
		public int Likes { get; set; }

		[DataMember]
		[XmlElement(Namespace = "https://mintplayer.com/music")]
		public int Dislikes { get; set; }

		[DataMember]
		[XmlElement(Namespace = "https://mintplayer.com/music")]
		public bool? Like { get; set; }
		
		[DataMember]
		[XmlElement(Namespace = "https://mintplayer.com/music")]
		public bool Authenticated { get; set; }
	}
}
