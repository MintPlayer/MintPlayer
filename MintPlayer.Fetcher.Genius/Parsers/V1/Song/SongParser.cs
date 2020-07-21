using System;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using MintPlayer.Fetcher.Dtos;
using MintPlayer.Fetcher.Genius.Helpers;

namespace MintPlayer.Fetcher.Genius.Parsers.V1.Song
{
    internal interface ISongParser
    {
        Task<Subject> ParseSong(string html, string pageData, bool trimTrash);
    }
    internal class SongParser : ISongParser
    {
        private readonly ILyricsTrimmer lyricsTrimmer;
        public SongParser(ILyricsTrimmer lyricsTrimmer)
        {
            this.lyricsTrimmer = lyricsTrimmer;
        }

        public async Task<Subject> ParseSong(string html, string pageData, bool trimTrash)
        {
            var data = JsonConvert.DeserializeObject<SongData>(pageData);
            if(data.LyricsData == null)
            {
                var rgxLyrics = new Regex(@"\<div class\=\""SongPageGrid-.*?\""\>(?<content>.*?)\<\/div\>\s*\<div class\=\""SectionLeaderboard__Container", RegexOptions.Singleline | RegexOptions.Multiline);
                var mLyrics = rgxLyrics.Match(html);
                if (mLyrics.Success)
                {
                    data.Song.Lyrics = mLyrics.Groups["content"].Value;
                }
            }
            else if(data.LyricsData.Body.Html == null)
            {

            }
            else
            {
                data.Song.Lyrics = await lyricsTrimmer.Trim(data.LyricsData.Body.Html, trimTrash);
            }
            return data.Song.ToDto();
        }
    }
}
