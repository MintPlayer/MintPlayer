using MintPlayer.Fetcher.Dtos;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MintPlayer.Fetcher.Genius.Parsers.V1.Artist
{
    internal interface IArtistParser
    {
        Task<Subject> ParseArtist(string pageData);
    }
    internal class ArtistParser : IArtistParser
    {
        private readonly IRequestSender requestSender;
        public ArtistParser(IRequestSender requestSender)
        {
            this.requestSender = requestSender;
        }

        public async Task<Subject> ParseArtist(string pageData)
        {
            var data = JsonConvert.DeserializeObject<ArtistData>(pageData);

            var songs = new List<Shared.Song>();
            var albums = new List<Shared.Album>();
            var page = (int?)1;
            var songs_structure = new
            {
                meta = new
                {
                    status = 0
                },
                response = new
                {
                    next_page = (int?)0,
                    songs = new List<Shared.Song>()
                }
            };
            var albums_structure = new
            {
                meta = new
                {
                    status = 0
                },
                response = new
                {
                    next_page = (int?)0,
                    albums = new List<Shared.Album>()
                }
            };

            while (true)
            {
                var json_songs = await requestSender.SendRequest($"https://genius.com/api/artists/{data.Artist.Id}/songs?per_page=50&page={page}&sort=popularity");
                var data_songs = JsonConvert.DeserializeAnonymousType(json_songs, songs_structure);
                songs.AddRange(data_songs.response.songs);

                if ((page = data_songs.response.next_page) == null)
                    break;
            }

            page = 1;
            while (true)
            {
                var json_albums = await requestSender.SendRequest($"https://genius.com/api/artists/{data.Artist.Id}/albums?per_page=50&page={page}");
                var data_albums = JsonConvert.DeserializeAnonymousType(json_albums, albums_structure);
                albums.AddRange(data_albums.response.albums);

                if ((page = data_albums.response.next_page) == null)
                    break;
            }

            data.Songs = songs;
            data.Albums = albums;

            return data.ToDto();
        }
    }
}
