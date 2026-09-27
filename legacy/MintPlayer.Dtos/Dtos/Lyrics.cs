using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace MintPlayer.Dtos.Dtos
{
	[XmlRoot(Namespace = "https://mintplayer.com/music")]
	[XmlType(Namespace = "https://mintplayer.com/music")]
	[DataContract(Namespace = "https://mintplayer.com/music")]
    public class Lyrics
    {
		[DataMember]
		[XmlElement(Namespace = "https://mintplayer.com/music")]
        public string Text { get; set; }

		[DataMember]
		[XmlElement(Namespace = "https://mintplayer.com/music")]
        public List<double> Timeline { get; set; }
    }
}
