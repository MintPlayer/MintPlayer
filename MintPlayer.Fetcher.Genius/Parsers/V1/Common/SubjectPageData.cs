﻿using Newtonsoft.Json;

namespace MintPlayer.Fetcher.Genius.Parsers.V1.Common
{
	internal class SubjectPageData
	{
		[JsonProperty("page_type")]
		public string PageType { get; set; }
	}
}
