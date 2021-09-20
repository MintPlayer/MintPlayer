using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MintPlayer.Fetcher.Genius.Abstractions.Parsers.Helpers;
using MintPlayer.Fetcher.Genius.Abstractions.Parsers.V1.Parsers;
using Newtonsoft.Json;

namespace MintPlayer.Fetcher.Genius.Parsers.V1.Parsers
{
	internal class ArtistV1Parser : IArtistV1Parser
	{
		private readonly ILdJsonReader ldJsonReader;
		private readonly HttpClient httpClient;
		public ArtistV1Parser(ILdJsonReader ldJsonReader, HttpClient httpClient)
		{
			this.ldJsonReader = ldJsonReader;
			this.httpClient = httpClient;
		}

		public async Task<MintPlayer.Fetcher.Abstractions.Dtos.Artist> Parse(string html, string pageData)
		{
			var ldJson = await ldJsonReader.ReadLdJson(html);
			var json = JsonConvert.DeserializeObject<V1.LdJson.ArtistData>(ldJson);

			var result = new MintPlayer.Fetcher.Abstractions.Dtos.Artist
			{
				Id = await GetArtistId(html),
				Url = json.Url,
				Name = json.Name,
				ImageUrl = json.Image,
			};

			var songs = await GetAllSongs(result.Id);
			result.Songs = songs.Select(s => new MintPlayer.Fetcher.Abstractions.Dtos.Song
			{
				Id = s.Id,
				Url = s.Url,
				Title = s.Title,
			}).ToList();
			var albums = await GetAllAlbums(result.Id);
			result.Albums = albums.Select(a => new MintPlayer.Fetcher.Abstractions.Dtos.Album
			{
				Id = a.Id,
				Url = a.Url,
				Name = a.Name,
			}).ToList();

			return result;
		}

		private async Task<IEnumerable<Entities.Song>> GetAllSongs(long artistId)
		{
			var page = 1;
			var fetched = new List<Entities.Song>();
			while (true)
			{
				var url = $"https://genius.com/api/artists/{artistId}/songs?page={page}&sort=popularity";
				var response = await httpClient.GetAsync(url);
				if (response.IsSuccessStatusCode)
				{
					var content = await response.Content.ReadAsStringAsync();
					var data = JsonConvert.DeserializeObject<Data.SongPagingResult>(content);
					fetched.AddRange(data.Response.Songs);

					if (data.Response.NextPage == null) break;
					else page = data.Response.NextPage.Value;
				}
			}

			return fetched;
		}

		private async Task<IEnumerable<Entities.Album>> GetAllAlbums(long artistId)
		{
			var page = 1;
			var fetched = new List<Entities.Album>();
			while (true)
			{
				var url = $"https://genius.com/api/artists/{artistId}/albums?page={page}";
				var response = await httpClient.GetAsync(url);
				if (response.IsSuccessStatusCode)
				{
					var content = await response.Content.ReadAsStringAsync();
					var data = JsonConvert.DeserializeObject<Data.AlbumPagingResult>(content);
					fetched.AddRange(data.Response.Albums);

					if (data.Response.NextPage == null) break;
					else page = data.Response.NextPage.Value;
				}
			}

			return fetched;
		}

		private Task<long> GetArtistId(string html)
		{
			var rgx = new Regex(@"\<meta content=\""\/artists\/(?<id>[0-9]+)\"" name\=\""newrelic\-resource\-path\"" \/\>");
			var m = rgx.Match(html);
			if (!m.Success)
			{
				throw new Exception("Could not extract artist ID");
			}

			return Task.FromResult(Convert.ToInt64(m.Groups["id"].Value));
		}
	}
}
