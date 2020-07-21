using System;

namespace MintPlayer.Fetcher.Exceptions
{
    public class NoParserFoundException : Exception
    {
        public NoParserFoundException() : base("No parser found")
        {
        }

        public NoParserFoundException(string html) : base($"No parser found for html: {html}")
        {
        }
    }
}
