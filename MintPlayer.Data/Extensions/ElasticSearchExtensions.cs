using Microsoft.Extensions.DependencyInjection;
using MintPlayer.Dtos.Dtos;
using MintPlayer.Data.Options;
using Nest;
using System;
using System.Collections.Generic;
using System.Text;

namespace MintPlayer.Data.Extensions
{
	public static class ElasticSearchExtensions
	{
		public static IServiceCollection AddElasticSearch(this IServiceCollection services, Action<ElasticSearchOptions> options)
		{
			var opt = new ElasticSearchOptions();
			options.Invoke(opt);

			if (opt.Active)
			{
				var conn_settings = new ConnectionSettings(new Uri(opt.Url))
					.DefaultIndex(opt.DefaultIndex)
					.DefaultMappingFor<Person>(p_desc => p_desc
						.IndexName("person")
						.Ignore(p => p.Artists)
					)
					.DefaultMappingFor<Artist>(a_desc => a_desc
						.IndexName("artist")
						.Ignore(a => a.CurrentMembers)
						.Ignore(a => a.PastMembers)
						.Ignore(a => a.Songs)
					)
					.DefaultMappingFor<Song>(s_desc => s_desc
						.IndexName("song")
						.Ignore(s => s.Artists)
						.Ignore(s => s.UncreditedArtists)
						.Ignore(s => s.Lyrics)
					);
				
				var client = new ElasticClient(conn_settings);
				var response_people = client.Indices.Create("person", index => index
					.Map<Person>(map => map
						.Properties(props => props
							.Text(desc => desc.Name(person => person.FirstName))
							.Text(desc => desc.Name(person => person.LastName))
							.Date(desc => desc.Name(person => person.Born))
							.Date(desc => desc.Name(person => person.Died))
							.Completion(desc => desc.Name(p => p.NameSuggest))
						)
					)
				);
				var response_artists = client.Indices.Create("artist", index => index
					.Map<Artist>(map => map
						.Properties(props => props
							.Text(desc => desc.Name(artist => artist.Name))
							.Text(a_comp => a_comp.Name(a => a.Name))
							.Number(desc => desc.Name(artist => artist.YearStarted))
							.Number(desc => desc.Name(artist => artist.YearQuit))
							.Completion(desc => desc.Name(a => a.NameSuggest))
						)
					)
				);
				var response_songs = client.Indices.Create("song", index => index
					.Map<Song>(map => map
						.Properties(props => props.Text(desc => desc.Name(song => song.Title))
							.Completion(s_comp => s_comp.Name(s => s.TitleSuggest))
						)
					)
				);

				return services.AddSingleton<IElasticClient>(client);
			}
			else
			{
				return services.AddSingleton<IElasticClient>(new ElasticClient());
			}
		}
	}
}
