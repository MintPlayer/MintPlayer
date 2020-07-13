//#define SongMeanings
#define Genius
//#define Musixmatch
//#define LyricsCom
//#define SongtekstenNet
//#define AZLyrics
//#define SongLyrics
//#define LoloLyrics

using Microsoft.Extensions.DependencyInjection;
using MintPlayer.Fetcher.DependencyInjection;
using System;
using System.Net.Http;

namespace MintPlayer.Fetcher.Test
{
    class Program
    {
        static void Main(string[] args)
        {
            var services = new ServiceCollection();
            services
                .AddSingleton<HttpClient>()
                .AddFetcherContainer()
                .AddGeniusFetcher()
                .AddMusixmatchFetcher()
                .AddLyricsComFetcher()
                .AddSongtekstenNetFetcher()
                .AddSongMeaningsFetcher()
                .AddAZLyricsFetcher()
                .AddSongLyricsFetcher()
                .AddLoloLyricsFetcher();

            var provider = services.BuildServiceProvider();

            var fetcherContainer = provider.GetService<IFetcherContainer>();

#if SongMeanings

            #region Song
            var url_songmeanings_song = "https://songmeanings.com/songs/view/3530822107858693072/";
            var songmeanings_song = fetcherContainer.Fetch(url_songmeanings_song, true).Result;

            var url_songmeanings_song1 = "https://songmeanings.com/songs/view/3530822107859455860/";
            var songmeanings_song1 = fetcherContainer.Fetch(url_songmeanings_song1, true).Result;

            var url_songmeanings_song3 = "https://songmeanings.com/songs/view/3530822107858740205/";
            var songmeanings_song3 = fetcherContainer.Fetch(url_songmeanings_song3, true).Result;
            #endregion

            #region Artist
            var url_songmeanings_artist1 = "https://songmeanings.com/artist/view/songs/137438984319/";
            var songmeanings_artist1 = fetcherContainer.Fetch(url_songmeanings_artist1, true).Result;
            #endregion

#endif

#if Genius
            // OK

            #region Song - OK
            //var url_genius_song = "https://genius.com/Dario-g-sunchyme-lyrics";
            //var url_genius_song = "https://genius.com/The-weeknd-i-feel-it-coming-lyrics";
            var url_genius_song = "https://genius.com/Oasis-whatever-lyrics";
            var genius_song = fetcherContainer.Fetch(url_genius_song, true).Result;
            #endregion

            #region Artist - OK
            var url_genius_artist = "https://genius.com/artists/Daft-punk";
            var genius_artist = fetcherContainer.Fetch(url_genius_artist, true).Result;
            #endregion

            #region Album - OK
            var url_genius_album = "https://genius.com/albums/Daft-punk/Random-access-memories";
            var genius_album = fetcherContainer.Fetch(url_genius_album, true).Result;
            #endregion

#endif

#if Musixmatch
            // OK

            #region Song - OK
            //var url_musixmatch_song = "https://www.musixmatch.com/lyrics/Roddy-Ricch/The-Box";
            //var url_musixmatch_song = "https://www.musixmatch.com/lyrics/Daft-Punk-feat-Nile-Rodgers-Pharrell-Williams/Get-lucky";
            var url_musixmatch_song = "https://www.musixmatch.com/lyrics/Ariana-Grande/Love-Me-Harder-feat-The-Weeknd";
            var musixmatch_song = fetcherContainer.Fetch(url_musixmatch_song, true).Result;
            #endregion

            #region Artist - OK
            var url_musixmatch_artist = "https://www.musixmatch.com/artist/The-Weeknd";
            var musixmatch_artist = fetcherContainer.Fetch(url_musixmatch_artist, true).Result;
            #endregion

            #region Album - OK
            var url_musixmatch_album = "https://www.musixmatch.com/album/The-Weeknd/The-Weeknd-In-Japan";
            var musixmatch_album = fetcherContainer.Fetch(url_musixmatch_album, true).Result;
            #endregion

#endif

#if LyricsCom
            // OK

            #region Song - OK
            var url_lyricscom_song = "https://www.lyrics.com/lyric/29531449";
            var lyricscom_song = fetcherContainer.Fetch(url_lyricscom_song, true).Result;
            #endregion

            #region Artist - OK
            var url_lyricscom_artist = "https://www.lyrics.com/artist/Daft-Punk/168791";
            var lyricscom_artist = fetcherContainer.Fetch(url_lyricscom_artist, true).Result;
            #endregion

            #region Album - OK
            var url_lyricscom_album_1 = "https://www.lyrics.com/album/3810277/100-Greatest-Classic-Rock-Songs";
            var lyricscom_album_1 = fetcherContainer.Fetch(url_lyricscom_album_1, true).Result;
            #endregion

            #region Album - OK
            var url_lyricscom_album_2 = "https://www.lyrics.com/album/2860319/Life-is-But-a-Dream";
            var lyricscom_album_2 = fetcherContainer.Fetch(url_lyricscom_album_2, true).Result;
            #endregion

#endif

#if SongtekstenNet

            #region Song - OK
            //var url_songtekstennet_song = "https://songteksten.net/lyric/1818/11910/france-gall/ella-elle-la.html";
            //var url_songtekstennet_song = "https://www.lyrics.com/lyric/29812508/Quand+la+musique+est+bonne";
            var url_songtekstennet_song = "https://songteksten.net/lyric/97/96025/daft-punk/get-lucky.html";
            var songtekstennet_song = fetcherContainer.Fetch(url_songtekstennet_song, true).Result;
            #endregion

            #region Album - OK
            var url_songtekstennet_album = "https://songteksten.net/albums/album/235/103/red-hot-chili-peppers/californication.html";
            var songtekstennet_album = fetcherContainer.Fetch(url_songtekstennet_album, true).Result;
            #endregion

            #region Artist - OK
            //var url_songtekstennet_artist = "https://songteksten.net/artist/lyrics/97/daft-punk.html";
            var url_songtekstennet_artist = "https://songteksten.net/artist/lyrics/235/red-hot-chili-peppers.html";
            var songtekstennet_artist = fetcherContainer.Fetch(url_songtekstennet_artist, true).Result;
            #endregion

#endif

#if AZLyrics

            #region Song
            const string url_azlyrics_song = "https://www.azlyrics.com/lyrics/djkhaled/imtheone.html";
            var azlyrics_song = fetcherContainer.Fetch(url_azlyrics_song, true).Result;
            #endregion

            #region Artist
            const string url_azlyrics_artist = "https://www.azlyrics.com/d/daftpunk.html";
            var azlyrics_artist = fetcherContainer.Fetch(url_azlyrics_artist, true).Result;
            #endregion

#endif

#if SongLyrics

            #region Song
            const string url_songlyrics_song = "http://www.songlyrics.com/dj-khaled-feat-justin-bieber-quavo-chance-the-rapper-lil-wayne/i-m-the-one-lyrics/";
            var songlyrics_song = fetcherContainer.Fetch(url_songlyrics_song, true).Result;
            #endregion

            #region Artist
            const string url_songlyrics_artist = "http://www.songlyrics.com/red-hot-chilli-peppers-lyrics/";
            var songlyrics_artist = fetcherContainer.Fetch(url_songlyrics_artist, true).Result;
            #endregion

            #region Album
            const string url_songlyrics_album = "http://www.songlyrics.com/red-hot-chilli-peppers/live-to-air/";
            var songlyrics_album = fetcherContainer.Fetch(url_songlyrics_album, true).Result;
            #endregion

#endif

#if LoloLyrics

            #region Song
            var url_lololyrics_song = "https://www.lololyrics.com/lyrics/37950.html";
            //var url_lololyrics_song = "https://www.lololyrics.com/lyrics/32008.html";
            var lololyrics_song = fetcherContainer.Fetch(url_lololyrics_song, true).Result;
            #endregion

            #region Artist
            var url_lololyrics_artist = "https://www.lololyrics.com/artist/Daft+Punk";
            //var url_lololyrics_song = "https://www.lololyrics.com/lyrics/32008.html";
            var lololyrics_artist = fetcherContainer.Fetch(url_lololyrics_artist, true).Result;
            #endregion

#endif
        }
    }
}
