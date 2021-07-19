using Nest;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace MintPlayer.Dtos.Dtos
{
	[XmlRoot(Namespace = "https://mintplayer.com/music")]
	[XmlType(Namespace = "https://mintplayer.com/music")]
	[DataContract(Namespace = "https://mintplayer.com/music")]
    public class Song : Subject
    {
		[DataMember]
		[XmlElement(Namespace = "https://mintplayer.com/music")]
        public string Title { get; set; }
      
		[DataMember]
		[XmlElement(Namespace = "https://mintplayer.com/music")]
        public DateTime Released { get; set; }
        
		[DataMember]
		[XmlElement(Namespace = "https://mintplayer.com/music")]
        public Lyrics Lyrics { get; set; }

		[DataMember]
		[XmlElement(Namespace = "https://mintplayer.com/music")]
        public string YoutubeId { get; internal set; }

        [DataMember]
        [XmlElement(Namespace = "https://mintplayer.com/music")]
        public string DailymotionId { get; internal set; }

        [DataMember]
        [XmlElement(Namespace = "https://mintplayer.com/music")]
        public string VimeoId { get; internal set; }

        [DataMember]
		[XmlElement(Namespace = "https://mintplayer.com/music")]
        public List<PlayerInfo> PlayerInfos { get; internal set; }
        
		[DataMember]
		[XmlElement(Namespace = "https://mintplayer.com/music")]
        public string Description { get; internal set; }

		[DataMember]
		[XmlElement(Namespace = "https://mintplayer.com/music")]
        public List<Artist> Artists { get; set; }
        
		[DataMember]
		[XmlElement(Namespace = "https://mintplayer.com/music")]
        public List<Artist> UncreditedArtists { get; set; }

        [XmlIgnore]
        [JsonIgnore]
        [IgnoreDataMember]
        public CompletionField TitleSuggest => new CompletionField { Input = new[] { Title } };
    }
}
