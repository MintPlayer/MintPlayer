using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace MintPlayer.Fetcher.Genius.Parsers.V2.Data
{
	internal class SongEntities
	{
		[JsonProperty("artists")]
		public Dictionary<string, object> Artists { get; set; }

		//[JsonProperty("songs")]
		//public Dictionary<string, object> Songs { get; set; }
	}
}
