using System;

namespace MintPlayer.Fetcher.Exceptions
{
    public class NoFetcherFoundException : Exception
    {
        public NoFetcherFoundException() : base("No fetcher found.")
        {
        }

        public NoFetcherFoundException(string url) : base($"No fetcher found for url {url}")
        {
        }
    }
}
