using System;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace MintPlayer.Dtos.Dtos
{
	[XmlRoot(Namespace = "https://mintplayer.com/music")]
	[XmlType(Namespace = "https://mintplayer.com/music")]
	[DataContract(Namespace = "https://mintplayer.com/music")]
	public class User
	{
		[DataMember]
		[XmlElement(Namespace = "https://mintplayer.com/music")]
		public Guid Id { get; set; }

		[DataMember]
		[XmlElement(Namespace = "https://mintplayer.com/music")]
		public string UserName { get; set; }

		[DataMember]
		[XmlElement(Namespace = "https://mintplayer.com/music")]
		public string Email { get; set; }

		[DataMember]
		[XmlElement(Namespace = "https://mintplayer.com/music")]
		public string PictureUrl { get; set; }
	}
}
