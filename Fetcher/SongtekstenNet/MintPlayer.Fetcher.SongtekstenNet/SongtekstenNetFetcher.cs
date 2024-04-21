using MintPlayer.Fetcher.Abstractions.Dtos;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

[assembly: InternalsVisibleTo("MintPlayer.Fetcher.SongtekstenNet.Test")]
namespace MintPlayer.Fetcher.SongtekstenNet;

public interface ISongtekstenNetFetcher
{
	Task<Subject> Fetch(string url, bool trimTrash);
}

internal class SongtekstenNetFetcher : Fetcher, ISongtekstenNetFetcher
{
	private readonly HttpClient httpClient;
	public SongtekstenNetFetcher(HttpClient httpClient)
	{
		this.httpClient = httpClient;
	}

	public override IEnumerable<Regex> UrlRegex => [new Regex(@"https\:\/\/songteksten\.net\/.+")];

	public override async Task<Subject> Fetch(string url, bool trimTrash)
	{
		var html = await SendRequest(httpClient, url);
		var splitted = url.Split('/');

		if (url.StartsWith("https://songteksten.net/lyric/"))
		{
			return await ParseSong(url, html);
		}
		else if (url.StartsWith("https://songteksten.net/artist/"))
		{
			return await ParseArtist(url, html);
		}
		else if (url.StartsWith("https://songteksten.net/albums/"))
		{
			return await ParseAlbum(url, html);
		}
		else
		{
			throw new Exception();
		}
	}

	#region Private methods
	public Task<Subject> ParseArtist(string url, string html)
	{
		var splitted = url.Split('/');
		var artist = new Artist
		{
			Url = url,
			Id = Convert.ToInt32(splitted[splitted.Length - 2]),
			Name = ExtractArtistName(html),
			Songs = ExtractArtistSongs(html).Select(s => s.ToDto()).ToList(),
			Albums = ExtractArtistAlbums(html).Select(a => a.ToDto()).ToList()
		};
		return Task.FromResult<Subject>(artist);
	}
	public Task<Subject> ParseAlbum(string url, string html)
	{
		var url_splitted = url.Split('/');
		var artists = ExtractAlbumArtists(html);
		var album = new Album
		{
			Url = url,
			Id = Convert.ToInt32(url_splitted[url_splitted.Length - 3]),
			Name = ExtractAlbumTitle(html),
			Artist = artists.Count == 1 ? artists.First().ToDto() : null,
			Tracks = ExtractAlbumSongs(html).Select(s => s.ToDto()).ToList(),
			CoverArtUrl = ExtractCoverArtUrl(html),
			ReleaseDate = ExtractReleaseDate(html)
		};
		return Task.FromResult<Subject>(album);
	}
	public Task<Subject> ParseSong(string url, string html)
	{
		var splitted = url.Split('/');
		var artists = ExtractSongArtists(html).Select(a => a.ToDto());
		var song = new Song
		{
			Url = url,
			Id = Convert.ToInt32(splitted[splitted.Length - 3]),
			Lyrics = ExtractSongLyrics(html, true),
			Title = ExtractSongTitle(html),
			PrimaryArtist = artists.FirstOrDefault(),
			FeaturedArtists = artists.Skip(1).ToList()
		};
		return Task.FromResult<Subject>(song);
	}
	#endregion

	#region Song
	private string ExtractSongLyrics(string html, bool trimTrash)
	{
		var regex = new Regex(@"(?<=\<\/h1\>).*?(?=\<div)", RegexOptions.Singleline | RegexOptions.Multiline);
		var match = regex.Match(html);
		if (!match.Success) throw new Exception("No tag found");

		var whitespaces_stripped = match.Value.Replace("\r", "").Replace("\n", "").Replace("<br />", Environment.NewLine);

		if (trimTrash)
		{
			var stripBracketsRegex = new Regex(@"\[.*?\]\r\n", RegexOptions.Singleline | RegexOptions.Multiline);
			var brackets_stripped = stripBracketsRegex.Replace(whitespaces_stripped, "");
			return brackets_stripped;
		}
		else
		{
			return whitespaces_stripped;
		}
	}

	private string ExtractSongTitle(string html)
	{
		var breadcrumbRegex = new Regex(@"(?<=\<ol class=\""breadcrumb\""\>).*?(?=\<\/ol\>)", RegexOptions.Singleline | RegexOptions.Multiline);
		var breadcrumbMatch = breadcrumbRegex.Match(html);
		if (!breadcrumbMatch.Success) throw new Exception("No breadcrumb found");

		var bc = breadcrumbMatch.Value;
		var titleregex = new Regex(@"(?<=\<li\>).*?(?=\<\/li\>)", RegexOptions.Singleline | RegexOptions.Multiline);
		var titleMatches = titleregex.Matches(bc);
		if (titleMatches.Count == 0) throw new Exception("No title found");

		return titleMatches[titleMatches.Count - 1].Value;
	}

	private List<Data.Artist> ExtractSongArtists(string html)
	{
		var regexUl = new Regex(@"\<h3\>Artiesten\<\/h3\>\s*\<ul.*?\>\s*(.*?)\s*\<\/ul\>");
		var matchUl = regexUl.Match(html);
		if (!matchUl.Success) throw new Exception("No UL found");

		var regexLi = new Regex(@"\<li.*?\>.*?\<a href\=\""(?<url>.*?)\""\>(?<name>.*?)\<\/a\>\<\/li\>", RegexOptions.Singleline | RegexOptions.Multiline);
		var artist_matches = regexLi.Matches(matchUl.Value);
		var arr_matches = new Match[artist_matches.Count];
		artist_matches.CopyTo(arr_matches, 0);


		return arr_matches.Select(m =>
		{
			var url_split = m.Groups["url"].Value.Split('/');
			return new Data.Artist
			{
				Id = Convert.ToInt32(url_split[url_split.Length - 2]),
				Url = m.Groups["url"].Value,
				Name = m.Groups["name"].Value
			};
		}).ToList();
	}
	#endregion
	#region Album
	private string ExtractAlbumTitle(string html)
	{
		var h1Regex = new Regex(@"\<h1.*?\>(?<title>.*?)\<\/h1\>", RegexOptions.Singleline | RegexOptions.Multiline);
		var h1Match = h1Regex.Match(html);
		if (!h1Match.Success) throw new Exception("No H1 tag found");

		var title = h1Match.Groups["title"].Value;
		return title.Split(new[] { " - " }, StringSplitOptions.RemoveEmptyEntries).First();
	}
	private List<Data.Song> ExtractAlbumSongs(string html)
	{
		var songsRegex = new Regex(@"\<h1\>(?<artist_album>.*?)\<\/h1\>\s*\<ul class\=\""list\-unstyled\""\>\s*(?<tracks>.*?)\s*\<\/ul\>", RegexOptions.Singleline | RegexOptions.Multiline);
		var matchSongs = songsRegex.Match(html);
		if (!matchSongs.Success) throw new Exception("No ul found");

		var liRegex = new Regex(@"\<li.*?\>.*?\<a href\=\""(?<url>.*?)\""\>(?<title>.*?)\<\/a\>\<\/li\>", RegexOptions.Singleline | RegexOptions.Multiline);
		var songMatches = liRegex.Matches(matchSongs.Groups["tracks"].Value);
		var arr_matches = new Match[songMatches.Count];
		songMatches.CopyTo(arr_matches, 0);

		return arr_matches.Select(m =>
		{
			var url_split = m.Groups["url"].Value.Split('/');
			return new Data.Song
			{
				Id = Convert.ToInt32(url_split[url_split.Length - 3]),
				Url = m.Groups["url"].Value,
				Title = m.Groups["title"].Value
			};
		}).ToList();
	}
	private List<Data.Artist> ExtractAlbumArtists(string html)
	{
		var artistsRegex = new Regex(@"\<h3\>Artiesten\<\/h3\>\s*\<ul class\=\""list\-unstyled\""\>\s*(?<artists>.*?)\s*\<\/ul\>", RegexOptions.Singleline | RegexOptions.Multiline);
		var matchArtists = artistsRegex.Match(html);
		if (!matchArtists.Success) throw new Exception("No ul found");

		var liRegex = new Regex(@"\<li.*?\>.*?\<a href\=\""(?<url>.*?)\""\>(?<name>.*?)\<\/a\>\<\/li\>", RegexOptions.Singleline | RegexOptions.Multiline);
		var artistMatches = liRegex.Matches(matchArtists.Groups["artists"].Value);
		var arr_matches = new Match[artistMatches.Count];
		artistMatches.CopyTo(arr_matches, 0);

		return arr_matches.Select(m =>
		{
			var url_split = m.Groups["url"].Value.Split('/');
			return new Data.Artist
			{
				Id = Convert.ToInt32(url_split[url_split.Length - 2]),
				Url = m.Groups["url"].Value,
				Name = m.Groups["name"].Value
			};
		}).ToList();
	}
	private string ExtractCoverArtUrl(string html)
	{
		var regexCover = new Regex(@"\<img class\=\""img\-thumbnail img\-fullwidth\"" src\=\""(?<cover>.*?)\"" alt\="".*?\"" border\=\""0\""\s*\/>", RegexOptions.Singleline | RegexOptions.Multiline);
		var matchCover = regexCover.Match(html);
		if (!matchCover.Success) throw new Exception("No album cover found");

		return matchCover.Groups["cover"].Value;
	}
	private DateTime ExtractReleaseDate(string html)
	{
		var regexStats = new Regex(@"\<h3\>Statistieken\<\/h3\>\s*\<ul class\=\""list\-unstyled\""\>\s*(?<stats>.*?)\s*\<\/ul\>", RegexOptions.Singleline | RegexOptions.Multiline);
		var matchStats = regexStats.Match(html);
		if (!matchStats.Success) throw new Exception("No stats found");

		var liRegex = new Regex(@"\<li\>\<span class\=\""glyphicon glyphicon\-star\""\>\<\/span\>\s*(?<property>[\sA-Za-z]+?)\:\s*(?<value>.+?)\s*\<\/li\>", RegexOptions.Singleline | RegexOptions.Multiline);
		var liMatches = liRegex.Matches(matchStats.Value);
		var arr_matches = new Match[liMatches.Count];
		liMatches.CopyTo(arr_matches, 0);

		var dict = arr_matches.ToDictionary(m => m.Groups["property"].Value, m => m.Groups["value"].Value);
		return DateTime.ParseExact(dict["Release"], "dd-MM-yyyy", null);
	}
	#endregion
	#region Artist
	private string ExtractArtistName(string html)
	{
		var breadcrumbRegex = new Regex(@"(?<=\<ol class=\""breadcrumb\""\>).*?(?=\<\/ol\>)", RegexOptions.Singleline | RegexOptions.Multiline);
		var breadcrumbMatch = breadcrumbRegex.Match(html);
		if (!breadcrumbMatch.Success) throw new Exception("No breadcrumb found");

		var bc = breadcrumbMatch.Value;
		var liRegex = new Regex(@"(?<=\<li\>).*?(?=\<\/li\>)", RegexOptions.Singleline | RegexOptions.Multiline);
		var liMatches = liRegex.Matches(bc);
		if (liMatches.Count == 0) throw new Exception("No title found");

		var a = liMatches[liMatches.Count - 2].Value;
		var aRegex = new Regex(@"(?<=\<a.*?\>).*?(?=\<\/a\>)", RegexOptions.Singleline | RegexOptions.Multiline);
		var aMatch = aRegex.Match(a);
		if (!aMatch.Success) throw new Exception("No a tag found");

		return aMatch.Value;
	}
	private List<Data.Song> ExtractArtistSongs(string html)
	{
		var h1Regex = new Regex(@"\<\/h1\>", RegexOptions.Singleline | RegexOptions.Multiline);
		var main = h1Regex.Split(html)[1];

		var ulRegex = new Regex(@"\<ul class\=\""list-unstyled\""\>(?<lis>.*?)\<\/ul\>", RegexOptions.Singleline | RegexOptions.Multiline);
		var songsMatch = ulRegex.Match(main).Groups["lis"];

		var songRegex = new Regex(@"\<a href=\""(?<url>.*?)\"">(?<title>.*?)\<\/a\>");
		var songMatches = songRegex.Matches(songsMatch.Value);
		var arrSongsMatches = new Match[songMatches.Count];
		songMatches.CopyTo(arrSongsMatches, 0);

		return arrSongsMatches.Select(m =>
		{
			var parts = m.Groups["url"].Value.Split('/');
			return new Data.Song
			{
				Id = Convert.ToInt32(parts[parts.Length - 3]),
				Title = m.Groups["title"].Value,
				Url = m.Groups["url"].Value
			};
		}).ToList();
	}
	private List<Data.Album> ExtractArtistAlbums(string html)
	{
		var albumsRegex = new Regex(@"\<h3\>Albums\<\/h3\>\s*\<ul class\=\""list-unstyled\""\>(?<lis>.*?)\<\/ul\>", RegexOptions.Singleline | RegexOptions.Multiline);
		var albumsSection = albumsRegex.Match(html).Groups["lis"].Value;

		var albumRegex = new Regex(@"\<a href=\""(?<url>.*?)\"">(?<name>.*?)\s*\((?<released>[^)]+)\)<\/a\>", RegexOptions.Singleline | RegexOptions.Multiline);
		var albumMatches = albumRegex.Matches(albumsSection);
		var arrAlbumMatches = new Match[albumMatches.Count];
		albumMatches.CopyTo(arrAlbumMatches, 0);

		return arrAlbumMatches.Select(m =>
		{
			var parts = m.Groups["url"].Value.Split('/');
			return new Data.Album
			{
				Id = Convert.ToInt32(parts[parts.Length - 3]),
				Url = m.Groups["url"].Value,
				Name = m.Groups["name"].Value,
				Released = DateTime.ParseExact(m.Groups["released"].Value, "dd-MM-yyyy", System.Globalization.CultureInfo.InvariantCulture)
			};
		}).ToList();
	}
	#endregion
}
