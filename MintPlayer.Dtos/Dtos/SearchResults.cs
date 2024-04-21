using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace MintPlayer.Dtos.Dtos;

[XmlRoot(Namespace = "https://mintplayer.com/music")]
[XmlType(Namespace = "https://mintplayer.com/music")]
[DataContract(Namespace = "https://mintplayer.com/music")]
public class SearchResults
{
	[DataMember]
	[XmlElement(Namespace = "https://mintplayer.com/music")]
	public List<Person> People { get; set; }

	[DataMember]
	[XmlElement(Namespace = "https://mintplayer.com/music")]
	public List<Artist> Artists { get; set; }

	[DataMember]
	[XmlElement(Namespace = "https://mintplayer.com/music")]
	public List<Song> Songs { get; set; }

	[DataMember]
	[XmlElement(Namespace = "https://mintplayer.com/music")]
	public int Count
	{
		get
		{
			int people = 0, artists = 0, songs = 0;
			if (People != null) people = People.Count;
			if (Artists != null) artists = Artists.Count;
			if (Songs != null) songs = Songs.Count;

			return people + artists + songs;
		}
	}

	public Subject First()
	{
		if (People.Any()) return People.First();
		if (Artists.Any()) return Artists.First();
		if (Songs.Any()) return Songs.First();

		return null;
	}
}
