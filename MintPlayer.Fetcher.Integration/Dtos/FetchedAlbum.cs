using System;
using System.Collections.Generic;
using System.Text;

namespace MintPlayer.Fetcher.Integration.Dtos
{
	public class FetchedAlbum : FetchedSubject
	{
		public string Url { get; set; }
		public string Name { get; set; }
		public DateTime? ReleaseDate { get; set; }
		public string CoverArtUrl { get; set; }

		public FetchResult<FetchedArtist> Artist { get; set; }
		public List<FetchResult<FetchedSong>> Songs { get; set; }
	}
}
