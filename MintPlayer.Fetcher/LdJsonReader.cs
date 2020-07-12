using System;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace MintPlayer.Fetcher
{
    public interface ILdJsonReader
    {
        Task<string> ReadLdJson(string html);
    }
    internal class LdJsonReader : ILdJsonReader
    {
        public Task<string> ReadLdJson(string html)
        {
            return new Task<string>(() =>
            {
                var ldJsonRegex = new Regex(@"\<script .*? type=\""application\/ld\+json\"".*?\>(?<body>.*?)\<\/script\>", RegexOptions.Singleline | RegexOptions.Multiline);
                var ldJsonMatch = ldJsonRegex.Match(html);
                if (!ldJsonMatch.Success) throw new Exception("No ld+json tag found");

                return ldJsonMatch.Groups["body"].Value;
            });
        }
    }
}
