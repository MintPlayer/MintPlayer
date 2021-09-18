using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace MintPlayer.Fetcher.Genius.Parsers.V2.Data
{
	internal class MediumData
	{
		[JsonProperty("url")]
		public string Url { get; set; }

		[JsonProperty("type")]
		public string Type { get; set; }

		[JsonProperty("provider")]
		public string Provider { get; set; }
	}
}
