﻿using Microsoft.Extensions.DependencyInjection;

namespace MintPlayer.Fetcher.Genius.Parsers.V1.Mappers
{
	internal static class DependencyInjection
	{
		public static IServiceCollection AddV1Mappers(this IServiceCollection services)
		{
			return services
				.AddScoped<ArtistV1Mapper>()
				.AddScoped<SongV1Mapper>()
				.AddScoped<MediumV1Mapper>();
		}
	}
}
