using MintPlayer.Fetcher.Abstractions.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

[assembly: InternalsVisibleTo("MintPlayer.Fetcher.SongMeanings.Test")]
namespace MintPlayer.Fetcher.SongMeanings
{
	public interface ISongMeaningsFetcher
	{
		Task<Subject> Fetch(string url, bool trimTrash);
	}

	internal class SongMeaningsFetcher : Fetcher, ISongMeaningsFetcher
	{
        private readonly HttpClient httpClient;
        public SongMeaningsFetcher(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        public override IEnumerable<Regex> UrlRegex
        {
            get
            {
                return new[]
                {
                    new Regex(@"https\:\/\/(www\.){0,1}songmeanings\.com\/.+")
                };
            }
        }

        public override async Task<Subject> Fetch(string url, bool trimTrash)
        {
            var html = await SendRequest(httpClient, url);
            if (url.StartsWith("https://songmeanings.com/songs/view/"))
            {
                var song = await ParseSong(url, html);
                return song;
            }
            else if (url.StartsWith("https://songmeanings.com/artist/view/"))
            {
                var artist = await ParseArtist(url, html);
                return artist;
            }
            else if (url.StartsWith("https://songmeanings.com/albums/view/tracks/"))
            {
                var album = await ParseAlbum(url, html);
                return album;
            }
            else
            {
                throw new Exception();
            }
        }

        #region Private methods
        #region ParseSong
        private Task<Subject> ParseSong(string url, string html)
        {
            var artistName = ExtractSongArtist(html);
            var song = new Song
            {
                Url = url,
                Title = ExtractSongTitle(html, artistName.Name),
                Lyrics = ExtractSongLyrics(html),
                PrimaryArtist = artistName,
                FeaturedArtists = null
            };
            return Task.FromResult<Subject>(song);
        }

        private string ExtractSongTitle(string html, string artistName)
        {
            var rgx = new Regex($@"\<title\>{artistName} \- (?<title>.+) Lyrics \| SongMeanings\<\/title\>", RegexOptions.Singleline | RegexOptions.Multiline);
            var m = rgx.Match(html);
            var val = m.Groups["title"].Value;

            var rgxFeat = new Regex(@"(?<real_title>.+) \(feat\. .+\)", RegexOptions.Singleline);
            var mFeat = rgxFeat.Match(val);
            if (mFeat.Success)
            {
                return mFeat.Groups["real_title"].Value;
            }
            else
            {
                return val;
            }
        }
        private string ExtractSongLyrics(string html)
        {
            var rgx = new Regex(@"\<div class\=\""holder lyric\-box\""\>\s*(?<lyrics>.*?)\<div\s", RegexOptions.Singleline | RegexOptions.Multiline);
            var m = rgx.Match(html);
            var lyrics = m.Groups["lyrics"].Value;
            return lyrics.Replace("<br>", string.Empty).Replace("\n", Environment.NewLine);
        }
        private Artist ExtractSongArtist(string html)
        {
            var rgx = new Regex(@"\<h3\>More \<a href=\""(?<url>.*?)\"" title=\""(?<artist>.*?) Lyrics\"">\k<artist> Lyrics\<\/a\>\<\/h3\>", RegexOptions.Singleline | RegexOptions.Multiline);
            var m = rgx.Match(html);
            return new Artist
            {
                Url = m.Groups["url"].Value,
                Name = m.Groups["artist"].Value
            };
        }
        #endregion
        #region ParseAlbum
        private Task<Subject> ParseAlbum(string url, string html)
        {
            throw new NotImplementedException();
        }
        #endregion
        #region ParseArtist
        private async Task<Subject> ParseArtist(string url, string html)
        {
            var rgxIsSongs = new Regex(@"https://songmeanings.com/artist/view/songs/(?<artistid>[0-9]+)/", RegexOptions.Singleline);
            var rgxIsAlbums = new Regex(@"https://songmeanings.com/artist/view/discography/(?<artistid>[0-9]+)/", RegexOptions.Singleline);

            var mIsSongs = rgxIsSongs.Match(url);
            var mIsAlbums = rgxIsAlbums.Match(url);

            string htmlSongs = string.Empty;
            string htmlAlbums = string.Empty;

            if (mIsSongs.Success)
            {
                var artistId = mIsSongs.Groups["artistid"].Value;
                htmlSongs = html;
                htmlAlbums = await SendRequest(httpClient, $"https://songmeanings.com/artist/view/discography/{artistId}/");
            }
            else if (mIsAlbums.Success)
            {
                var artistId = mIsAlbums.Groups["artistid"].Value;
                htmlAlbums = html;
                htmlSongs = await SendRequest(httpClient, $"https://songmeanings.com/artist/view/discography/{artistId}/");
            }
            else
            {
                throw new Exception("Invalid URL");
            }

            var songs = await ExtractArtistSongs(htmlSongs);
            var albums = await ExtractArtistAlbums(htmlAlbums);

            return new Artist
            {
                Url = url,
                Name = await ExtractArtistName(html),
                Songs = songs.ToList(),
                Albums = albums.ToList()
            };
        }
        private Task<string> ExtractArtistName(string html)
        {
            var rgxName = new Regex(@"\<h2\>\<a href\=\""\/\/songmeanings\.com\/artist\/view\/songs\/[0-9]+\/\"" title\=\""(?<name>.+?)\""\>\k<name>\<\/a\>\<\/h2\>", RegexOptions.Singleline | RegexOptions.Multiline);
            var mName = rgxName.Match(html);
            if (mName.Success) return Task.FromResult(mName.Groups["name"].Value);
            else throw new Exception("Could not find artist name");
        }
        private Task<IEnumerable<Song>> ExtractArtistSongs(string songsHtml)
        {
            //var rgxSong = new Regex(@"\<tr id\=\""lyric\-(?<id>[0-9]+)\""\>\<td class\=\""\""\>\<a style\=\""\"" class\=\""\"" href\=\""(?<url>\/\/songmeanings\.com\/songs\/view\/\k<id>\/)\"" title\=\""(?<title>.*?)\""\>\k<title>\<\/a\>\<\/td\>\<td class\=\""\""\>\<span id\=\""lyriclinks-\k<id>\"" style\=\""display\: none;\""\>\<\/span\>\<\/td\>\<td class\=\""comments \"">\<a style\=\""\"" class\=\""\"" href\=\""\k<url>\"" title\=\""[0-9]+ comments on \k<title> lyrics\"" rel\=\""nofollow\""\>[0-9]+\<\/a\>\<\/td\>\<\/tr\>", RegexOptions.Singleline | RegexOptions.Multiline);
            //var mSongs = rgxSong.Matches(songsHtml);
            //var arrMatches = new Match[mSongs.Count];
            //mSongs.CopyTo(arrMatches, 0);
            //return Task.FromResult(arrMatches.Select(m => new Song
            //{
            //    Title = m.Groups["title"].Value,
            //    Url = m.Groups["url"].Value
            //}));

            var rgxTr = new Regex(@"\<tr id\=\""lyric\-(?<id>[0-9]+)\""\>(?<content>.*?)\<\/tr\>", RegexOptions.Singleline | RegexOptions.Multiline);
            var mTrs = rgxTr.Matches(songsHtml);
            var arrTrs = new Match[mTrs.Count];
            mTrs.CopyTo(arrTrs, 0);

            var rgxTd = new Regex(@"\<td.*?\>\<a .*? href\=\""(?<url>\/\/songmeanings\.com\/songs\/view\/[0-9]+\/)\"" title\=\""(?<title>.*?) lyrics\""\>\k<title>\<\/a\>\<\/td\>\<td.*?\>\<span .*?\>\<\/span\>\<\/td\>\<td class\=\""comments [a-z]*\"">\<a .*? href\=\""\k<url>\"" title\=\""[0-9]+ comments on \k<title> lyrics\"" rel\=\""nofollow\""\>[0-9]+\<\/a\>\<\/td\>", RegexOptions.Singleline | RegexOptions.Multiline);
            var mTds = arrTrs.Select(m => rgxTd.Match(m.Value));

            var result = mTds.Select(m => new Song {
                Url = m.Groups["url"].Value,
                Title = m.Groups["title"].Value
            });
            return Task.FromResult(result);
        }
        private Task<IEnumerable<Album>> ExtractArtistAlbums(string albumsHtml)
        {
            var rgxAlbum = new Regex(@"\<div class\=\""album\-holder albums\-list\""\>\s*\<div class\=\""info\""\>\s*\<img src\=\""(?<image>.*?)\"" alt\=\""(?<name>.*?)\"" .*?\>\s*\<\/div\>\s*\<div class\=\""text\""\>\s*\<h3\>\<a href\=\""(?<url>.*?)\"" title\=\""\k<name>\""\>\k<name>\<\/a\>\<\/h3\>\s*\<ul class\=\""albumlist\""\>\s*\<li\>(?<released>[0-9\-]+)\<\/li\>\s*\<li\>[0-9]+ Songs\<\/li\>\s*\<li\>\<a href\=\""(\k<url>.*?)\"" title\=\""See album details\""\>See album details\<\/a\>\<\/li\>\s*\<\/ul\>\s*\<\/div\>", RegexOptions.Singleline | RegexOptions.Multiline);
            var mAlbums = rgxAlbum.Matches(albumsHtml);
            var arrMatches = new Match[mAlbums.Count];
            mAlbums.CopyTo(arrMatches, 0);
            return Task.FromResult(arrMatches.Select(m => new Album
            {
                Name = m.Groups["name"].Value,
                CoverArtUrl = m.Groups["image"].Value,
                Url = m.Groups["url"].Value
            }));
        }
        #endregion
        #endregion
    }
}
