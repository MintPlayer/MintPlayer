using System;
using System.Collections.Generic;
using System.Text;

namespace MintPlayer.Fetcher.Integration.Dtos
{
	public class FetchedSong : FetchedSubject
    {
		public string Url { get; set; }
		public string Title { get; set; }
		public DateTime? Released { get; set; }
		public string Lyrics { get; set; }

		public List<FetchResult<FetchedArtist>> Artists { get; set; }
	}
}
