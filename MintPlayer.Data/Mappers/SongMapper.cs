using MintPlayer.Data.Helpers;

namespace MintPlayer.Data.Mappers;

internal interface ISongMapper
{
	MintPlayer.Dtos.Dtos.Song? Entity2Dto(Entities.Song? song, bool include_invisible_media, bool include_relations = false);
	Entities.Song? Dto2Entity(MintPlayer.Dtos.Dtos.Song? song, Entities.User user, MintPlayerContext mintplayer_context);
}

internal class SongMapper : ISongMapper
{
	private readonly IServiceProvider serviceProvider;
	public SongMapper(IServiceProvider serviceProvider)
	{
		this.serviceProvider = serviceProvider;
	}

	public MintPlayer.Dtos.Dtos.Song? Entity2Dto(Entities.Song? song, bool include_invisible_media, bool include_relations = false)
	{
		if (song == null)
		{
			return null;
		}


		var songHelper = serviceProvider.GetRequiredService<SongHelper>();
		var playerInfos = songHelper.GetPlayerInfos(song.Media).ToList();

		var result = new MintPlayer.Dtos.Dtos.Song
		{
			Id = song.Id,
			Title = song.Title,
			Released = song.Released,

			Description = song.Description,
			YoutubeId = playerInfos.FirstOrDefault(p => p.Type == MintPlayer.Dtos.Enums.EPlayerType.Youtube)?.Id,
			DailymotionId = playerInfos.FirstOrDefault(p => p.Type == MintPlayer.Dtos.Enums.EPlayerType.DailyMotion)?.Id,
			VimeoId = playerInfos.FirstOrDefault(p => p.Type == MintPlayer.Dtos.Enums.EPlayerType.Vimeo)?.Id,
			SoundCloudUrl = playerInfos.FirstOrDefault(p => p.Type == MintPlayer.Dtos.Enums.EPlayerType.SoundCloud)?.Id,
			PlayerInfos = playerInfos,

			Text = song.Text,
			DateUpdate = song.DateUpdate ?? song.DateInsert,
			ConcurrencyStamp = Convert.ToBase64String(song.ConcurrencyStamp),
		};

		if (include_relations)
		{
			if (song.Lyrics != null)
			{
				var lastLyric = song.Lyrics.OrderBy(l => l.UpdatedAt).LastOrDefault();
				result.Lyrics = new MintPlayer.Dtos.Dtos.Lyrics
				{
					Text = lastLyric?.Text,
					Timeline = lastLyric?.Timeline
				};
			}

			if (song.Artists != null)
			{
				var artistMapper = serviceProvider.GetRequiredService<IArtistMapper>();
				result.Artists = song.Artists
					.Where(@as => @as.Credited)
					.Select(@as => artistMapper.Entity2Dto(@as.Artist, false))
					.ToList();
				result.UncreditedArtists = song.Artists
					.Where(@as => !@as.Credited)
					.Select(@as => artistMapper.Entity2Dto(@as.Artist, false))
					.ToList();
			}

			if (song.Media != null)
			{
				var mediumMapper = serviceProvider.GetRequiredService<IMediumMapper>();
				result.Media = song.Media
					.Where(m => m.Type.Visible | include_invisible_media)
					.Select(medium => mediumMapper.Entity2Dto(medium, true))
					.ToList();
			}

			if (song.Tags != null)
			{
				var tagMapper = serviceProvider.GetRequiredService<ITagMapper>();
				result.Tags = song.Tags
					.Select(st => tagMapper.Entity2Dto(st.Tag))
					.ToList();
			}
		}

		return result;
	}

	/// <summary>Only use this method for creation of a song</summary>
	public Entities.Song? Dto2Entity(MintPlayer.Dtos.Dtos.Song? song, Entities.User user, MintPlayerContext mintplayer_context)
	{
		if (song == null) return null;
		var entity_song = new Entities.Song
		{
			Id = song.Id,
			Title = song.Title,
			Released = song.Released
		};
		if (song.Artists != null)
		{
			entity_song.Artists = song.Artists.Select(artist =>
			{
				var entity_artist = mintplayer_context.Artists.Find(artist.Id);
				return new Entities.ArtistSong(entity_artist, entity_song);
			}).ToList();
		}
		entity_song.Lyrics = new List<Entities.Lyrics>(new[] {
			new Entities.Lyrics
			{
				Song = entity_song,
				User = user,
				Text = song.Lyrics.Text,
				Timeline = song.Lyrics.Timeline
			},
		});
		if (song.Media != null)
		{
			var mediumMapper = serviceProvider.GetRequiredService<IMediumMapper>();
			entity_song.Media = song.Media.Select(m =>
			{
				var medium = mediumMapper.Dto2Entity(m, mintplayer_context);
				medium.Subject = entity_song;
				return medium;
			}).ToList();
		}
		#region Tags
		if (song.Tags != null)
		{
			entity_song.Tags = song.Tags.Select(t =>
			{
				var tag = mintplayer_context.Tags.Find(t.Id);
				return new Entities.SubjectTag(entity_song, tag);
			}).ToList();
		}
		#endregion

		return entity_song;
	}
}
