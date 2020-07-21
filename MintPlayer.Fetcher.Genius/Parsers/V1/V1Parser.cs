using MintPlayer.Fetcher.Dtos;
using MintPlayer.Fetcher.Genius.Parsers.V1;
using Newtonsoft.Json;
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MintPlayer.Fetcher.Genius.Parsers.V1
{
    internal interface IV1Parser : IParser
    {
    }
    internal class V1Parser : IV1Parser
    {
        private readonly Artist.IArtistParser artistParser;
        private readonly Album.IAlbumParser albumParser;
        private readonly Song.ISongParser songParser;
        public V1Parser(Artist.IArtistParser artistParser, Album.IAlbumParser albumParser, Song.ISongParser songParser)
        {
            this.artistParser = artistParser;
            this.albumParser = albumParser;
            this.songParser = songParser;
        }
        
        public Task<bool> IsMatch(string html)
        {
            var isMatch = html.Contains("__PRELOADED_STATE__");
            return Task.FromResult(isMatch);
        }

        public async Task<Subject> Parse(string html, bool trimTrash)
        {
            var page_data = await ReadPageData(html);

            var structure = new { currentPage = string.Empty };
            var subject = JsonConvert.DeserializeAnonymousType(page_data, structure);

            switch (subject.currentPage)
            {
                case "profile":
                    return await artistParser.ParseArtist(page_data);
                case "songPage":
                    return await songParser.ParseSong(html, page_data, trimTrash);
                case "album":
                    return await albumParser.ParseAlbum(page_data);
                default:
                    throw new Exception("Type not recognized");
            }
        }

        private Task<string> ReadPageData(string html)
        {
            //var pageDataRegex = new Regex(@"(?<=\<meta content\=\"")(.*?)(?=\""\sitemprop\=\""page_data\""\>\<\/meta\>)");
            var pageDataRegex = new Regex(@"window\.__PRELOADED_STATE__\s\=\sJSON\.parse\(\'(?<data>.*?)\'\)\;");

            var pageData = pageDataRegex.Match(html).Groups["data"].Value;
            var fixedPageData = pageData
                .Replace(@"\""", @"""")
                .Replace(@"\\", @"\")
                .Replace("&quot;", @"""")
                .Replace("&amp;", "&")
                .Replace("&lt;", "<")
                .Replace("&gt;", ">")
                .Replace("&#39;", "'");

            return Task.FromResult(fixedPageData);
        }
    }
}
