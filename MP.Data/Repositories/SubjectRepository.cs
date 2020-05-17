using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MintPlayer.Dtos.Dtos;
using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MintPlayer.Data.Repositories
{
	internal interface ISubjectRepository
    {
        Task<Tuple<int, int>> GetLikes(int subjectId);
        Task<bool?> DoesLike(int subjectId);
        Task Like(int subjectId, bool like);
		Task<IEnumerable<Subject>> GetLikedSubjects();

		Task<IEnumerable<Subject>> Suggest(string[] subjects, string search_term, bool include_relations, bool include_invisible_media);
		Task<IEnumerable<Subject>> Search(string[] subjects, string search_term, bool exact, bool include_relations, bool include_invisible_media);

		Task SaveChangesAsync();
    }
	internal class SubjectRepository : ISubjectRepository
	{
		private IHttpContextAccessor http_context;
		private MintPlayerContext mintplayer_context;
		private UserManager<Entities.User> user_manager;
		private IElasticClient elastic_client;
		private IConfiguration configuration;
		public SubjectRepository(IHttpContextAccessor http_context, MintPlayerContext mintplayer_context, UserManager<Entities.User> user_manager, IElasticClient elastic_client, IConfiguration configuration)
		{
			this.http_context = http_context;
			this.mintplayer_context = mintplayer_context;
			this.user_manager = user_manager;
			this.elastic_client = elastic_client;
			this.configuration = configuration;
		}

		public async Task<Tuple<int, int>> GetLikes(int subjectId)
		{
			var likes = await mintplayer_context.Likes
				.Where(l => l.SubjectId == subjectId)
				.Where(l => l.DoesLike)
				.CountAsync();
			var dislikes = await mintplayer_context.Likes
				.Where(l => l.SubjectId == subjectId)
				.Where(l => !l.DoesLike)
				.CountAsync();
			return new Tuple<int, int>(likes, dislikes);
		}

		public async Task<bool?> DoesLike(int subjectId)
		{
			var user = await user_manager.GetUserAsync(http_context.HttpContext.User);
			var user_id = user == null ? Guid.Empty : user.Id;

			if (user_id == Guid.Empty) throw new UnauthorizedAccessException();

			var like = await mintplayer_context.Likes
				.Where(l => l.SubjectId == subjectId)
				.Where(l => l.UserId == user_id)
				.FirstOrDefaultAsync();

			if (like == null) return null;
			else return like.DoesLike;
		}

		public async Task Like(int subjectId, bool like)
		{
			var user = await user_manager.GetUserAsync(http_context.HttpContext.User);
			var subject_db = await mintplayer_context.FindAsync<Entities.Subject>(subjectId);

			var existing_like = await mintplayer_context.Likes
				.Where(l => l.SubjectId == subjectId)
				.Where(l => l.UserId == user.Id)
				.FirstOrDefaultAsync();

			if (existing_like == null)
			{
				var new_like = new Entities.Like(subject_db, user, like);
				await mintplayer_context.AddAsync(new_like);
			}
			else
			{
				existing_like.DoesLike = like;
				mintplayer_context.Update(existing_like);
			}
		}

		public async Task<IEnumerable<Subject>> GetLikedSubjects()
		{

			var user = await user_manager.GetUserAsync(http_context.HttpContext.User);
			if (user == null) throw new UnauthorizedAccessException();

			var subjects = mintplayer_context.Subjects
				.Where(s => s.Likes.Any(l => l.User == user && l.DoesLike))
				.Select(s => ToDto(s, false, false));

			return subjects;
		}


		public async Task<IEnumerable<Subject>> Suggest(string[] subjects, string search_term, bool include_relations, bool include_invisible_media)
		{
			IEnumerable<Subject> person_options = new Person[0], artist_options = new Artist[0], song_options = new Song[0];

			if (configuration.GetValue<bool>("ElasticSearch:Active"))
			{
				#region People
				if (subjects.Contains("person"))
				{
					var people = await elastic_client.SearchAsync<Person>(p => p
						.Suggest(su => su
							.Completion("person-completion", p_desc => p_desc
								.Field(f => f.NameSuggest)
								.Prefix(search_term)
								.Fuzzy(f => f
									.Fuzziness(Fuzziness.Auto)
								)
								.Analyzer("simple")
								.Size(5)
							)
						)
					);
					person_options = people.Suggest.Values.SelectMany(
						suggest => suggest.SelectMany(
							s => s.Options.Select(o => o.Source)
						)
					).Cast<Subject>();
				}
				#endregion
				#region Artists
				if (subjects.Contains("artist"))
				{
					var artists = await elastic_client.SearchAsync<Artist>(a => a
						.Suggest(su => su
							.Completion("artist-completion", a_desc => a_desc
								.Field(f => f.NameSuggest)
								.Prefix(search_term)
								.Fuzzy(f => f
									.Fuzziness(Fuzziness.Auto)
								)
								.Analyzer("simple")
								.Size(5)
							)
						)
					);
					artist_options = artists.Suggest.Values.SelectMany(
						suggest => suggest.SelectMany(
							s => s.Options.Select(o => o.Source)
						)
					).Cast<Subject>();
				}
				#endregion
				#region Songs
				if (subjects.Contains("song"))
				{
					var songs = await elastic_client.SearchAsync<Song>(s => s
						.Suggest(su => su
							.Completion("song-completion", s_desc => s_desc
								.Field(f => f.TitleSuggest)
								.Prefix(search_term)
								.Fuzzy(f => f
									.Fuzziness(Fuzziness.Auto)
								)
								.Analyzer("simple")
								.Size(5)
							)
						)
					);
					song_options = songs.Suggest.Values.SelectMany(
						suggest => suggest.SelectMany(
							s => s.Options.Select(o => o.Source)
						)
					).Cast<Subject>();
				}
				#endregion
			}
			else
			{
				#region People
				if (subjects.Contains("person"))
				{
					if (include_relations)
					{
						person_options = mintplayer_context.People
							.Include(p => p.Artists)
								.ThenInclude(ap => ap.Artist)
							.Where(p => (p.FirstName + " " + p.LastName).Contains(search_term))
							.Select(p => ToDto(p, include_relations, include_invisible_media));
					}
					else
					{
						person_options = mintplayer_context.People
							.Where(p => (p.FirstName + " " + p.LastName).Contains(search_term))
							.Select(p => ToDto(p, include_relations, include_invisible_media));
					}
				}
				#endregion
				#region Artists
				if (subjects.Contains("artist"))
				{
					if (include_relations)
					{
						artist_options = mintplayer_context.Artists
							.Include(a => a.Members)
								.ThenInclude(ap => ap.Person)
							.Include(a => a.Songs)
								.ThenInclude(@as => @as.Song)
							.Where(a => a.Name.Contains(search_term))
							.Select(a => ToDto(a, include_relations, include_invisible_media));
					}
					else
					{
						artist_options = mintplayer_context.Artists
							.Where(a => a.Name.Contains(search_term))
							.Select(a => ToDto(a, include_relations, include_invisible_media));
					}
				}
				#endregion
				#region Songs
				if (subjects.Contains("song"))
				{
					if (include_relations)
					{
						song_options = mintplayer_context.Songs
							.Include(s => s.Artists)
								.ThenInclude(@as => @as.Artist)
							.Where(s => s.Title.Contains(search_term))
							.Select(s => ToDto(s, include_relations, include_invisible_media));
					}
					else
					{
						song_options = mintplayer_context.Songs
							.Where(s => s.Title.Contains(search_term))
							.Select(s => ToDto(s, include_relations, include_invisible_media));
					}
				}
				#endregion
			}

			var result = person_options.Union(artist_options).Union(song_options);
			return result;
		}

		public async Task<IEnumerable<Subject>> Search(string[] subjects, string search_term, bool exact, bool include_relations, bool include_invisible_media)
		{
			IEnumerable<Subject> person_results = new Person[0], artist_results = new Artist[0], song_results = new Song[0];

			if (configuration.GetValue<bool>("ElasticSearch:Active"))
			{
				if (subjects.Contains("person"))
				{
					var people = await elastic_client.SearchAsync<Person>(
						a => a.Query(q1 => q1.MultiMatch(
							mm => mm.Query(search_term).Fields(m => m.Fields(f => f.FirstName, f => f.LastName)).Fuzziness(exact ? Fuzziness.EditDistance(0) : Fuzziness.Auto)
						))
					);
					person_results = people.Documents.Cast<Subject>();
				}
				if (subjects.Contains("artist"))
				{
					var artists = await elastic_client.SearchAsync<Artist>(
						a => a.Query(q1 => q1.MultiMatch(
							mm => mm.Query(search_term).Fields(m => m.Fields(f => f.Name)).Fuzziness(exact ? Fuzziness.EditDistance(0) : Fuzziness.Auto).PrefixLength(1)
						))
					);
					artist_results = artists.Documents.Cast<Subject>();
				}
				if (subjects.Contains("song"))
				{
					var songs = await elastic_client.SearchAsync<Song>(
						a => a.Query(q1 => q1.MultiMatch(
							mm => mm.Query(search_term).Fields(m => m.Fields(f => f.Title)).Fuzziness(exact ? Fuzziness.EditDistance(0) : Fuzziness.Auto).PrefixLength(1)
						))
					);
					song_results = songs.Documents.Cast<Subject>();
				}
			}
			else
			{
				#region People
				if (subjects.Contains("person"))
				{
					if (include_relations)
					{
						person_results = mintplayer_context.People
							.Include(p => p.Artists)
								.ThenInclude(ap => ap.Artist)
							.Where(p => (p.FirstName + " " + p.LastName) == search_term)
							.Select(p => ToDto(p, include_relations, include_invisible_media));
					}
					else
					{
						person_results = mintplayer_context.People
							.Where(p => (p.FirstName + " " + p.LastName) == search_term)
							.Select(p => ToDto(p, include_relations, include_invisible_media));
					}
				}
				#endregion
				#region Artists
				if (subjects.Contains("artist"))
				{
					if (include_relations)
					{
						artist_results = mintplayer_context.Artists
							.Include(a => a.Members)
								.ThenInclude(ap => ap.Person)
							.Include(a => a.Songs)
								.ThenInclude(@as => @as.Song)
							.Where(a => a.Name == search_term)
							.Select(a => ToDto(a, include_relations, include_invisible_media));
					}
					else
					{
						artist_results = mintplayer_context.Artists
							.Where(a => a.Name == search_term)
							.Select(a => ToDto(a, include_relations, include_invisible_media));
					}
				}
				#endregion
				#region Songs
				if (subjects.Contains("song"))
				{
					if (include_relations)
					{
						song_results = mintplayer_context.Songs
							.Include(s => s.Artists)
								.ThenInclude(@as => @as.Artist)
							.Where(s => s.Title == search_term)
							.Select(s => ToDto(s, include_relations, include_invisible_media));
					}
					else
					{
						song_results = mintplayer_context.Songs
							.Where(s => s.Title == search_term)
							.Select(s => ToDto(s, include_relations, include_invisible_media));
					}
				}
				#endregion
			}
			
			var result = person_results.Union(artist_results).Union(song_results);
			return result;
		}

		public async Task SaveChangesAsync()
		{
			await mintplayer_context.SaveChangesAsync();
		}

		#region Conversion methods
		internal static Subject ToDto(Entities.Subject subject, bool include_relations, bool include_invisible_media)
		{
			if (subject == null) return null;

			var subject_type = subject.GetType();
			if (subject_type == typeof(Entities.Person))
			{
				return PersonRepository.ToDto((Entities.Person)subject, include_relations, include_invisible_media);
			}
			else if (subject_type == typeof(Entities.Artist))
			{
				return ArtistRepository.ToDto((Entities.Artist)subject, include_relations, include_invisible_media);
			}
			else if (subject_type == typeof(Entities.Song))
			{
				return SongRepository.ToDto((Entities.Song)subject, include_relations, include_invisible_media);
			}
			else
			{
				throw new ArgumentException("The subject type was not recognized", nameof(subject));
			}
		}
		#endregion
	}
}
