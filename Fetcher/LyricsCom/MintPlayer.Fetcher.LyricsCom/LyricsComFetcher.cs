using MintPlayer.Fetcher.Abstractions.Dtos;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

[assembly: InternalsVisibleTo("MintPlayer.Fetcher.LyricsCom.Test")]
namespace MintPlayer.Fetcher.LyricsCom
{
	public interface ILyricsComFetcher
	{
		Task<Subject> Fetch(string url, bool trimTrash);
	}

	internal class LyricsComFetcher : Fetcher, ILyricsComFetcher
	{
        private readonly HttpClient httpClient;
        public LyricsComFetcher(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        public override IEnumerable<Regex> UrlRegex
        {
            get
            {
                return new[]
                {
                    new Regex(@"https\:\/\/www\.lyrics\.com\/.+")
                };
            }
        }

        public override async Task<Subject> Fetch(string url, bool trimTrash)
        {
            var html = await SendRequest(httpClient, url);
            if (url.StartsWith("https://www.lyrics.com/lyric/"))
            {
                var song = await ParseSong(url, html);
                return song;
            }
            else if (url.StartsWith("https://www.lyrics.com/artist/"))
            {
                var artist = await ParseArtist(url, html);
                return artist;
            }
            else if (url.StartsWith("https://www.lyrics.com/album/"))
            {
                var album = await ParseAlbum(url, html);
                return album;
            }
            else
            {
                throw new Exception("URL cannot be mapped");
            }
        }

        #region Private methods

        #region ParseSong
        private Task<Subject> ParseSong(string url, string html)
        {
            var artists = ExtractSongArtists(html);

            var result = new Song
            {
                Id = ExtractSongId(url),
                Url = url,
                Title = ExtractSongTitle(html),
                PrimaryArtist = artists.FirstOrDefault(),
                FeaturedArtists = artists.Skip(1).ToList(),
                Lyrics = ExtractSongLyrics(html, true),
                Media = new List<Medium>()
            };

            var youtube = ExtractSongYoutubeUrl(html);
            if (youtube != null)
			{
				result.Media.Add(new Medium { Type = MintPlayer.Fetcher.Abstractions.Enums.EMediumType.YouTube, Value = youtube });
			}

			return Task.FromResult<Subject>(result);
        }

        private int ExtractSongId(string url)
        {
            var parts = url.Split('/');

            int result;
            var success = int.TryParse(parts[4], out result);
            if (!success) Debugger.Break();
            return result;
        }

        private string ExtractSongTitle(string html)
        {
            var h1Regex = new Regex(@"\<h1.*?\>(?<title>.*?)\<\/h1\>", RegexOptions.Singleline | RegexOptions.Multiline);
            var h1Match = h1Regex.Match(html);
            if (!h1Match.Success) throw new Exception("No H1 tag found");

            return h1Match.Groups["title"].Value;
        }

        private string ExtractSongLyrics(string html, bool trimTrash)
        {
            var preRegex = new Regex(@"\<pre.*?\>(?<body>.*?)\<\/pre\>", RegexOptions.Singleline | RegexOptions.Multiline);
            var preMatch = preRegex.Match(html);
            if (!preMatch.Success) throw new Exception("No pre tag found");

            var stripARegex = new Regex(@"\<a.*?\>|\<\/a\>", RegexOptions.Singleline | RegexOptions.Multiline);
            var stripped = stripARegex.Replace(preMatch.Groups["body"].Value, "");
            var whitespaces_stripped = stripped.Replace("\r\n", Environment.NewLine).Trim();

            return whitespaces_stripped;
        }

        private string ExtractSongYoutubeUrl(string html)
        {
            var idRegex = new Regex(@"\<div class=\""youtube-player\"" data-id=\""(?<id>.*?)\""\>\<\/div\>", RegexOptions.Singleline | RegexOptions.Multiline);
            var idMatch = idRegex.Match(html);
            if (!idMatch.Success) return null;

            var id = idMatch.Groups["id"].Value;

            return $"http://www.youtube.com/watch?v={id}";
        }

        private IEnumerable<Artist> ExtractSongArtists(string html)
        {
            var h3Regex = new Regex(@"\<h3 class=\""lyric-artist\""\>(?<h3>.*?)\<\/h3\>", RegexOptions.Singleline | RegexOptions.Multiline);
            var h3Match = h3Regex.Match(html);
            if (!h3Match.Success) return null;

            var aRegex = new Regex(@"\<a href=\""(?<url>artist\/.*?)\""\>(?<name>.*?)\<\/a\>", RegexOptions.Singleline | RegexOptions.Multiline);
            var aMatches = aRegex.Matches(h3Match.Groups["h3"].Value);

            var aMatchesArray = new Match[aMatches.Count];
            aMatches.CopyTo(aMatchesArray, 0);

            return aMatchesArray.Select(m => new Artist
            {
                Name = m.Groups["name"].Value,
                Url = "https://www.lyrics.com/" + m.Groups["url"].Value
            });
        }
        #endregion

        #region ParseArtist
        private Task<Subject> ParseArtist(string url, string html)
        {
            var albums = ExtractArtistAlbums(url, html);

            var result = new Artist
            {
                Id = ExtractArtistId(url),
                Name = ExtractArtistName(html),
                Url = url,
                ImageUrl = ExtractArtistImage(html),
                Albums = albums.ToList(),
                Songs = albums.SelectMany(a => a.Tracks).ToList()
            };

            return Task.FromResult<Subject>(result);
        }

        private int ExtractArtistId(string url)
        {
            var parts = url.Split('/');

            int result;
            var success = int.TryParse(parts.Last(), out result);
            if (!success) Debugger.Break();
            return result;
        }

        private string ExtractArtistName(string html)
        {
            var h1Regex = new Regex(@"\<h1.*?\>\<a href=\""(?<url>.*?)\>(?<name>.*?)\<\/a\>\<\/h1\>", RegexOptions.Singleline | RegexOptions.Multiline);
            var h1Match = h1Regex.Match(html);
            if (!h1Match.Success) throw new Exception("No H1 tag found");

            return h1Match.Groups["name"].Value;
        }

        private string ExtractArtistImage(string html)
        {
            var rgx_div_avatar = new Regex(@"\<div id=\""featured\-artist\-avatar\"" .*?\>\<img src=\""(?<image>.*?)\"" class=\""artist\-thumb\""\>\<\/div\>", RegexOptions.Multiline | RegexOptions.Singleline | RegexOptions.IgnoreCase);
            var match_image = rgx_div_avatar.Match(html);
            if (match_image.Success) return match_image.Groups["image"].Value;
            else return null;
        }

        private IEnumerable<Album> ExtractArtistAlbums(string url, string html)
        {
            var rgx_container = new Regex(@"\<div class=\""tdata\-ext\""\>\s*(?<albums>.*?)\s*\<\/div\>\s*\<section.*?\>", RegexOptions.Singleline | RegexOptions.Multiline);
            var match_albums = rgx_container.Match(html);
            if (!match_albums.Success) throw new Exception("No albums section found");

            var rgx_album = new Regex(@"\<div class=\""clearfix\""\>\<h3 class=\""artist\-album\-label\""\>\<a href=\""(?<album_url>.*?)\""\>(?<album_title>.*?)\<\/a\>\s*\<span class=\""year\""\>\[(?<album_year>[0-9]{4})\]\<\/span\>\<\/h3\>\<div class=\""artist\-album\-thumb\""\>\<img src=\""(?<album_image>.*?)\""\>\<\/div\>\<table class=\""tdata\""\>.*?\<tbody\>(?<rows>.*?)\<\/tbody\>\<\/table\>\<\/div\>", RegexOptions.Singleline | RegexOptions.Multiline);
            var album_matches = rgx_album.Matches(match_albums.Groups["albums"].Value);
            var arr_albums = new Match[album_matches.Count];
            album_matches.CopyTo(arr_albums, 0);

            return arr_albums.Select(a => new Album
            {
                Id = Convert.ToInt32(a.Groups["album_url"].Value.Split('/').Reverse().ElementAt(1)),
                Url = "https://www.lyrics.com" + a.Groups["album_url"].Value,
                //Url = "https://www.lyrics.com" + HttpUtility.UrlDecode(a.Groups["album_url"].Value),
                Name = a.Groups["album_title"].Value,
                ReleaseDate = new DateTime(Convert.ToInt32(a.Groups["album_year"].Value), 1, 1),
                CoverArtUrl = a.Groups["album_image"].Value,

                Tracks = ExtractArtistAlbumSongs(a.Groups["rows"].Value).ToList()
            });
        }

        private IEnumerable<Song> ExtractArtistAlbumSongs(string html)
        {
            var rgx_song = new Regex(@"\<tr\>\<td.*?\>\<strong\>\<a href=\""(?<url>.*?)\""\>(?<title>.*?)\<\/a\>", RegexOptions.Singleline | RegexOptions.Multiline);
            var m_song = rgx_song.Matches(html);
            var matches = new Match[m_song.Count];
            m_song.CopyTo(matches, 0);

            return matches.Select(m => new Song
            {
                Title = m.Groups["title"].Value,
                Url = "https://www.lyrics.com" + m.Groups["url"].Value
                //Url = "https://www.lyrics.com" + HttpUtility.UrlDecode(m.Groups["url"].Value)
            });
        }

        #endregion

        #region ParseAlbum
        private Task<Subject> ParseAlbum(string url, string html)
        {
            var year = ExtractReleasedYear(html);

            var result = new Album
            {
                Id = Convert.ToInt32(url.Split('/').Reverse().ElementAt(1)),
                Url = url,
                Name = ExtractAlbumTitle(html),
                Artist = ExtractAlbumArtist(html),
                CoverArtUrl = ExtractAlbumCoverArt(html),
                ReleaseDate = year == 0 ? (DateTime?)null : new DateTime(year, 1, 1),
                Tracks = ExtractAlbumTracks(html).ToList()
            };

            return Task.FromResult<Subject>(result);
        }

        private string ExtractAlbumTitle(string html)
        {
            var rgx_title = new Regex(@"\<h1\>\<strong\>(?<title>.*) Album\<\/strong\>\<\/h1\>", RegexOptions.Singleline | RegexOptions.Multiline);
            var m_title = rgx_title.Match(html);

            if (m_title.Success) return m_title.Groups["title"].Value;
            else return null;
        }

        private IEnumerable<Song> ExtractAlbumTracks(string html)
        {
            var rgx_table = new Regex(@"\<tbody\>(?<rows>.*)\<\/tbody\>", RegexOptions.Singleline | RegexOptions.Multiline);
            var m_table = rgx_table.Match(html);

            if (!m_table.Success) throw new Exception("Track table not found");

            var rgx_track = new Regex(@"\<tr\>\<td .*?\>(?<disc_number>[0-9]+)\<\/td\>\<td .*?\>(?<track_number>[0-9]+)\<\/td\>\<td .*?\>\<div\>\<strong\>\<a href=\""(?<url>.*?)\""\>(?<title>.*?)\<\/a\>\<\/strong\>\<\/div\>\<\/td\>\<td .*?\>(?<duration>[0-9\:]+)\<\/td\>\<td .*?\>.*?\<\/td\>\<\/tr\>", RegexOptions.Singleline | RegexOptions.Multiline);
            var m_tracks = rgx_track.Matches(m_table.Groups["rows"].Value);
            var track_matches = new Match[m_tracks.Count];
            m_tracks.CopyTo(track_matches, 0);

            return track_matches.Select(m => new Song
            {
                Id = Convert.ToInt32(m.Groups["url"].Value.Split('/').Reverse().ElementAt(1)),
                Url = "https://www.lyrics.com" + m.Groups["url"].Value,
                Title = m.Groups["title"].Value
            });
        }

        private Artist ExtractAlbumArtist(string html)
        {
            var rgx_artist = new Regex(@"\<h1\>.*\<\/h1\>\s*\<h2\>(?<artist>.*?)\<\/h2\>", RegexOptions.Singleline | RegexOptions.Multiline);
            var m_artist = rgx_artist.Match(html);

            if (!m_artist.Success) throw new Exception("No h2 tag found");

            var content = m_artist.Groups["artist"].Value;
            if (content == "Various Artists") return null;

            var rgx_a = new Regex(@"\<a href=\""(?<url>.*?)\""\>(?<name>.*?)\<\/a\>", RegexOptions.Singleline);
            var m_a = rgx_a.Match(content);

            if (!m_a.Success) throw new Exception("Pattern match for album artist failed");

            return new Artist
            {
                Name = m_a.Groups["name"].Value,
                Url = "https://www.lyrics.com/" + HttpUtility.UrlDecode(m_a.Groups["url"].Value)
            };
        }

        private string ExtractAlbumCoverArt(string html)
        {
            var rgx_cover = new Regex(@"\<div class=\""album\-meta\-thumb\"" .*?\>\<img .*? src=\""(?<image>.*?)"" .*?\>\<\/div\>", RegexOptions.Singleline | RegexOptions.Multiline);
            var m_cover = rgx_cover.Match(html);

            if (m_cover.Success) return m_cover.Groups["image"].Value;
            else return null;
        }

        private int ExtractReleasedYear(string html)
        {
            var rgx_year = new Regex(@"\<a .*?\>(?<year>[0-9]{4})\<\/a\>", RegexOptions.Singleline | RegexOptions.Multiline);
            var m_year = rgx_year.Match(html);

            if (m_year.Success) return Convert.ToInt32(m_year.Groups["year"].Value);
            else return 0;
        }
        #endregion

        #endregion
    }
}
