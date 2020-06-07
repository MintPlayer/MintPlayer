using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text.RegularExpressions;
using MintPlayer.Data.Entities.Interfaces;
using MintPlayer.Data.Extensions;

namespace MintPlayer.Data.Entities
{
	internal class Song : Subject, ISoftDelete
	{
		public string Title { get; set; }
		public DateTime Released { get; set; }

		#region Text
		[NotMapped]
		public override string Text => Title;
		#endregion
		#region YoutubeId
		[NotMapped]
		public string YoutubeId
		{
			get
			{
				if (Media == null) return null;

				var youtubeVideo = Media.FirstOrDefault(m => m.Type.PlayerType == MintPlayer.Dtos.Enums.ePlayerType.Youtube);
				return ParseYoutubeId(youtubeVideo);
			}
		}
		#endregion
		#region DailymotionId
		[NotMapped]
		public string DailymotionId
		{
			get
			{
				if (Media == null) return null;

				var dailymotionVideo = Media.FirstOrDefault(m => m.Type.PlayerType == MintPlayer.Dtos.Enums.ePlayerType.DailyMotion);
				return ParseDailymotionId(dailymotionVideo);
			}
		}
		#endregion
		#region PlayerInfo
		[NotMapped]
		public MintPlayer.Dtos.Dtos.PlayerInfo PlayerInfo
		{
			get
			{
				if (Media == null) return null;
				var video = Media.FirstOrDefault(m => m.Type.PlayerType != MintPlayer.Dtos.Enums.ePlayerType.None);
				if (video == null) return null;

				switch (video.Type.PlayerType)
				{
					case MintPlayer.Dtos.Enums.ePlayerType.Youtube:
						return new MintPlayer.Dtos.Dtos.PlayerInfo
						{
							Id = YoutubeId,
							Type = MintPlayer.Dtos.Enums.ePlayerType.Youtube
						};
					case MintPlayer.Dtos.Enums.ePlayerType.DailyMotion:
						return new MintPlayer.Dtos.Dtos.PlayerInfo
						{
							Id = DailymotionId,
							Type = MintPlayer.Dtos.Enums.ePlayerType.DailyMotion
						};
					default:
						throw new Exception("Unexpected value for PlayerType");
				}
			}
		}
		#endregion
		#region Description
		[NotMapped]
		public string Description
		{
			get
			{
				var hasArtists = Artists == null ? false : Artists.Any();
				if (hasArtists)
					return $"{Title} - {string.Join(" & ", Artists.Select(@as => @as.Artist.Name))}";
				else
					return Title;
			}
		}
		#endregion


		public List<ArtistSong> Artists { get; set; }
		public List<Lyrics> Lyrics { get; set; }
		public List<PlaylistSong> Tracks { get; set; }

		#region Helper methods
		private string ParseYoutubeId(Medium youtubeVideo)
		{
			if (youtubeVideo == null) return null;

			var m1 = Regex.Match(youtubeVideo.Value, "http[s]{0,1}://youtu.be/(?<id>.+)$");
			if (m1.Success) return m1.Groups["id"].Value;

			var m2 = Regex.Match(youtubeVideo.Value, @"http[s]{0,1}://www.youtube.com/watch\?(?<query>.+)$");
			if (m2.Success)
			{
				var query = m2.Groups["query"].Value.AsQueryString();
				if (query.ContainsKey("v")) return query["v"];
				else return null;
			}

			var m3 = Regex.Match(youtubeVideo.Value, @"http[s]{0,1}://m.youtube.com/watch\?(?<query>.+)$");
			if (m3.Success)
			{
				var query = m3.Groups["query"].Value.AsQueryString();
				if (query.ContainsKey("v")) return query["v"];
				else return null;
			}
			else
			{
				return null;
			}
		}
		private string ParseDailymotionId(Medium dailymotionVideo)
		{
			if (dailymotionVideo == null) return null;

			// https://www.dailymotion.com/video/x2yhuhb
			var m1 = Regex.Match(dailymotionVideo.Value, @"http[s]{0,1}\:\/\/www\.dailymotion\.com\/video\/(?<id>.+)$");
			if (m1.Success) return m1.Groups["id"].Value;
			else return null;
		}
		#endregion
	}
}
