using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyInjection;

namespace MintPlayer.Fetcher.Genius.Parsers.V1.Common.Mappers
{
	internal static class DependencyInjection
	{
		public static IServiceCollection AddV1Mappers(this IServiceCollection services)
		{
			return services;
		}
	}
}
