using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using MintPlayer.Fetcher.Dtos;
using MintPlayer.Fetcher.Exceptions;

namespace MintPlayer.Fetcher.Genius.Parsers
{
    internal interface IGeniusParser
    {
        Task<Subject> Parse(string html, bool trimTrash);
    }
    internal class GeniusParser : IGeniusParser
    {
        private readonly IEnumerable<IGeniusVersionParser> geniusParsers;
        public GeniusParser(IEnumerable<IGeniusVersionParser> geniusParsers)
        {
            this.geniusParsers = geniusParsers;
        }

        public async Task<Subject> Parse(string html, bool trimTrash)
        {
            var parser = geniusParsers.FirstOrDefault(p => p.IsMatch(html).Result);

            if (parser == null)
                throw new NoParserFoundException(html);

            var result = await parser.Parse(html, trimTrash);
            return result;
        }
    }
}
