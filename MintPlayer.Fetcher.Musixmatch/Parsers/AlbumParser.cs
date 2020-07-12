using System;
using System.Linq;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using MintPlayer.Fetcher.Dtos;

namespace MintPlayer.Fetcher.Musixmatch.Parsers
{
    internal interface IAlbumParser
    {
        Task<Subject> ParseAlbum(string url, string html);
    }
    internal class AlbumParser : IAlbumParser
    {
        public Task<Subject> ParseAlbum(string url, string html)
        {
            var rgx_json = new Regex(@"var __mxmState = (?<json>.*?);\<\/script\>", RegexOptions.Singleline | RegexOptions.Multiline);
            var m_json = rgx_json.Match(html);
            if (!m_json.Success) throw new Exception("ParseAlbum rgx match failed");

            Json.SongData data = null;
            try
            {
                data = JsonConvert.DeserializeObject<Json.SongData>(m_json.Groups["json"].Value);
            }
            catch (Exception ex)
            {
                Debugger.Break();
            }

            var result = new Album
            {
                Id = data.Page.Album.Id,
                Name = data.Page.Album.Name,
                Artist = ExtractAlbumArtist(html),
                ReleaseDate = data.Page.Album.ReleaseDate,
                CoverArtUrl = FirstNotEmpty(data.Page.Album.CoverArt800, data.Page.Album.CoverArt500, data.Page.Album.CoverArt350, data.Page.Album.CoverArt100),
                Tracks = data.Page.Tracks.List.Select(t => new Song
                {
                    Id = t.Id,
                    Title = t.Name,
                    Url = t.ShareUrl.Split('?')[0]
                }).ToList(),
                Url = url
            };
            return Task.FromResult<Subject>(result);
        }
        private string FirstNotEmpty(params string[] items)
        {
            return items.FirstOrDefault(i => !string.IsNullOrEmpty(i));
        }
        private Artist ExtractAlbumArtist(string html)
        {
            var rgx_h2 = new Regex(@"\<h2 itemProp=\""byArtist\"".*?\>(?<artistinfo>.*?)\<\/h2\>", RegexOptions.Singleline | RegexOptions.Multiline);
            var match_h2 = rgx_h2.Match(html);
            var rgx_info = new Regex(@"\<a href=\""(?<url>.*?)\""\>(?<name>.*?)\<\/a\>", RegexOptions.Singleline | RegexOptions.Multiline);
            var match_info = rgx_info.Match(match_h2.Value);
            return new Artist
            {
                Name = match_info.Groups["name"].Value,
                Url = "https://musixmatch.com" + match_info.Groups["url"].Value
            };
        }
    }
}
