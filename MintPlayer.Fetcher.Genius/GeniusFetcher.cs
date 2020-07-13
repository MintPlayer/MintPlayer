using MintPlayer.Fetcher.Dtos;
using MintPlayer.Fetcher.Genius.Parsers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MintPlayer.Fetcher.Genius
{
    internal class GeniusFetcher : Fetcher
    {
        private readonly IRequestSender requestSender;
        private readonly IV1Parser v1Parser;
        private readonly IV2Parser v2Parser;
        public GeniusFetcher(IRequestSender requestSender, IV1Parser v1Parser, IV2Parser v2Parser)
        {
            this.requestSender = requestSender;
            this.v1Parser = v1Parser;
            this.v2Parser = v2Parser;
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

            //var html = System.IO.File.ReadAllText("whatever.html");
            if (html.Contains("__PRELOADED_STATE__"))
            {
                var result = await v1Parser.Parse(html, trimTrash);
                return result;
            }
            else
            {
                var result = await v2Parser.Parse(html, trimTrash);
                return result;
            }
        }
    }
}
