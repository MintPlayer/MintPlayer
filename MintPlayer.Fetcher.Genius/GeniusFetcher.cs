using MintPlayer.Fetcher.Dtos;
using MintPlayer.Fetcher.Genius.Parsers;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MintPlayer.Fetcher.Genius
{
    internal class GeniusFetcher : Fetcher
    {
        private readonly IRequestSender requestSender;
        private readonly IGeniusParser geniusParser;
        public GeniusFetcher(IRequestSender requestSender, IGeniusParser geniusParser)
        {
            this.requestSender = requestSender;
            this.geniusParser = geniusParser;
        }

        public override IEnumerable<Regex> UrlRegex
        {
            get
            {
                return new[] {
                    new Regex(@"https:\/\/genius\.com\/.+")
                };
            }
        }

        public override async Task<Subject> Fetch(string url, bool trimTrash)
        {
            var html = await requestSender.SendRequest(url);
            var result = await geniusParser.Parse(html, trimTrash);
            return result;
        }
    }
}
