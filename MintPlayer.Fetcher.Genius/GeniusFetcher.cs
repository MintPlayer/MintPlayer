using MintPlayer.Fetcher.Dtos;
using MintPlayer.Fetcher.Exceptions;
using MintPlayer.Fetcher.Genius.Parsers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MintPlayer.Fetcher.Genius
{
    internal class GeniusFetcher : Fetcher
    {
        private readonly IRequestSender requestSender;
        private readonly Parsers.V1.IV1Parser v1Parser;
        private readonly Parsers.V2.IV2Parser v2Parser;
        private readonly Parsers.V3.IV3Parser v3Parser;
        public GeniusFetcher(IRequestSender requestSender, Parsers.V1.IV1Parser v1Parser, Parsers.V2.IV2Parser v2Parser, Parsers.V3.IV3Parser v3Parser)
        {
            this.requestSender = requestSender;
            this.v1Parser = v1Parser;
            this.v2Parser = v2Parser;
            this.v3Parser = v3Parser;
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

            var parsers = new IParser[] { v1Parser, v2Parser, v3Parser };
            var parser = parsers.FirstOrDefault(p => p.IsMatch(html).Result);

            if(parser == null)
                throw new NoParserFoundException(html);

            var result = await parser.Parse(html, trimTrash);
            return result;
        }
    }
}
