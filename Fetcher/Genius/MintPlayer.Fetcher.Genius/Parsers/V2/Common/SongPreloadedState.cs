using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace MintPlayer.Fetcher.Genius.Parsers.V2.Common
{
	internal class SongPreloadedState
	{
		[JsonProperty("currentPage")]
		public string CurrentPage { get; set; }
	}
}
