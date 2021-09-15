using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MintPlayer.Fetcher.Genius.Abstractions.Parsers.V1.Song
{
	public interface ISongV1Parser
	{
		Task<Fetcher.Abstractions.Dtos.Song> Parse(string html, string pageData, bool trimTrash);
	}
}
