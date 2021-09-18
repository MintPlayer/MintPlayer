using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using MintPlayer.Fetcher.Abstractions.Dtos;

namespace MintPlayer.Fetcher.Genius.Parsers.V1.Mappers
{
	internal class MediumV1Mapper
	{
		public MediumV1Mapper()
		{
		}

		public Task<Medium> ToDto(Data.MediumData medium)
		{
			if (medium == null)
			{
				return null;
			}

			var result = new Medium
			{
				Value = medium.Url,
			};

			switch (medium.Provider)
			{
				case "youtube":
					result.Type = MintPlayer.Fetcher.Abstractions.Enums.eMediumType.YouTube;
					break;
				case "soundcloud":
					result.Type = MintPlayer.Fetcher.Abstractions.Enums.eMediumType.SoundCloud;
					break;
				case "spotify":
					result.Type = MintPlayer.Fetcher.Abstractions.Enums.eMediumType.Spotify;
					break;
			}

			return Task.FromResult(result);
		}
	}
}
