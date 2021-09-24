using System;

namespace MintPlayer.Fetcher.Genius.Exceptions
{
	public class NoFeaturingArtistsFoundException : Exception
	{
		public NoFeaturingArtistsFoundException(string message) : base(message)
		{
		}
	}
}
