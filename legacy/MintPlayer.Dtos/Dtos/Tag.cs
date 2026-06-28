using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace MintPlayer.Dtos.Dtos
{
	[XmlRoot(Namespace = "https://mintplayer.com/music")]
	[XmlType(Namespace = "https://mintplayer.com/music")]
	[DataContract(Namespace = "https://mintplayer.com/music")]
    public class Tag
    {
		[DataMember]
		[XmlElement(Namespace = "https://mintplayer.com/music")]
        public int Id { get; set; }

		[DataMember]
		[XmlElement(Namespace = "https://mintplayer.com/music")]
        public string Description { get; set; }

		[DataMember]
		[XmlElement(Namespace = "https://mintplayer.com/music")]
        public TagCategory Category { get; set; }

		[DataMember]
		[XmlElement(Namespace = "https://mintplayer.com/music")]
        public List<Subject> Subjects { get; set; }

		[DataMember]
		[XmlElement(Namespace = "https://mintplayer.com/music")]
        public Tag Parent { get; set; }
        
		[DataMember]
		[XmlElement(Namespace = "https://mintplayer.com/music")]
        public List<Tag> Children { get; set; }

		[DataMember]
		[XmlElement(Namespace = "https://mintplayer.com/music")]
        public string Text => Description;
    }
}
