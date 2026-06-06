using Nest;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using System.Xml.Serialization;

namespace MintPlayer.Dtos.Dtos
{
	[XmlRoot(Namespace = "https://mintplayer.com/music")]
	[XmlType(Namespace = "https://mintplayer.com/music")]
	[DataContract(Namespace = "https://mintplayer.com/music")]
	public class Artist : Subject
	{
		[DataMember]
		[XmlElement(Namespace = "https://mintplayer.com/music")]
		public string Name { get; set; }
		
		[DataMember]
		[XmlElement(Namespace = "https://mintplayer.com/music")]
		public int? YearStarted { get; set; }
		
		[DataMember]
		[XmlElement(Namespace = "https://mintplayer.com/music")]
		public int? YearQuit { get; set; }

		[DataMember]
		[XmlElement(Namespace = "https://mintplayer.com/music")]
		public List<Person> PastMembers { get; set; }
		
		[DataMember]
		[XmlElement(Namespace = "https://mintplayer.com/music")]
		public List<Person> CurrentMembers { get; set; }
		
		[DataMember]
		[XmlElement(Namespace = "https://mintplayer.com/music")]
		public List<Song> Songs { get; set; }

		[XmlIgnore]
		[JsonIgnore]
		[IgnoreDataMember]
		public CompletionField NameSuggest => new CompletionField { Input = new[] { Name } };
	}
}
