﻿using System.Threading.Tasks;
using MintPlayer.Fetcher.Abstractions.Dtos;

namespace MintPlayer.Fetcher.Genius.Abstractions.Parsers.V2
{
	public interface ISongV2Parser
	{
		Task<Song> Parse(string html, string preloadedState);
	}
}
