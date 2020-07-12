using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using MintPlayer.Fetcher.Dtos;

namespace MintPlayer.Fetcher.SongMeanings.Parsers
{
    internal interface IArtistParser
    {
        Task<Subject> ParseArtist(string url, string html);
    }

    internal class ArtistParser : IArtistParser
    {
        private readonly IRequestSender requestSender;
        public ArtistParser(IRequestSender requestSender)
        {
            this.requestSender = requestSender;
        }

        public async Task<Subject> ParseArtist(string url, string html)
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
                htmlAlbums = await requestSender.SendRequest($"https://songmeanings.com/artist/view/discography/{artistId}/");
            }
            else if (mIsAlbums.Success)
            {
                var artistId = mIsAlbums.Groups["artistid"].Value;
                htmlAlbums = html;
                htmlSongs = await requestSender.SendRequest($"https://songmeanings.com/artist/view/discography/{artistId}/");
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

            var result = mTds.Select(m => new Song
            {
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
    }
}
