using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using MintPlayer.Data.Dtos;
using MintPlayer.Data.Repositories.Interfaces;
using Nest;

namespace MintPlayer.Data.Repositories
{
    internal class SubjectRepository : ISubjectRepository
    {
        private IHttpContextAccessor http_context;
        private MintPlayerContext mintplayer_context;
        private UserManager<Entities.User> user_manager;
        private Nest.IElasticClient elastic_client;
        public SubjectRepository(IHttpContextAccessor http_context, MintPlayerContext mintplayer_context, UserManager<Entities.User> user_manager, Nest.IElasticClient elastic_client)
        {
            this.http_context = http_context;
            this.mintplayer_context = mintplayer_context;
            this.user_manager = user_manager;
            this.elastic_client = elastic_client;
        }

        public async Task<List<Subject>> Suggest(string[] subjects, string search_term)
        {
            IEnumerable<Subject> person_options = new Person[0], artist_options = new Artist[0], song_options = new Song[0];

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

            return person_options.Union(artist_options).Union(song_options).ToList();
        }

        public async Task<List<Subject>> Search(string[] subjects, string search_term, bool fuzzy)
        {
            IEnumerable<Subject> person_results = new Person[0], artist_results = new Artist[0], song_results = new Song[0];
            if (subjects.Contains("person"))
            {
                var people = await elastic_client.SearchAsync<Person>(
                    a => a.Query(q1 => q1.MultiMatch(
                        mm => mm.Query(search_term)
                            .Fields(m => m.Fields(f => f.FirstName, f => f.LastName))
                            .Fuzziness(fuzzy ? Fuzziness.Auto : Fuzziness.EditDistance(0))
                            .PrefixLength(1)
                    ))
                );
                person_results = people.Documents.Cast<Subject>();
            }
            if (subjects.Contains("artist"))
            {
                var artists = await elastic_client.SearchAsync<Artist>(
                    a => a.Query(q1 => q1.MultiMatch(
                        mm => mm.Query(search_term)
                            .Fields(m => m.Fields(f => f.Name))
                            .Fuzziness(fuzzy ? Fuzziness.Auto : Fuzziness.EditDistance(0))
                            .PrefixLength(1)
                    ))
                );
                artist_results = artists.Documents.Cast<Subject>();
            }
            if (subjects.Contains("song"))
            {
                var songs = await elastic_client.SearchAsync<Song>(
                    a => a.Query(q1 => q1.MultiMatch(
                        mm => mm.Query(search_term)
                            .Fields(m => m.Fields(f => f.Title))
                            .Fuzziness(fuzzy ? Fuzziness.Auto : Fuzziness.EditDistance(0))
                            .PrefixLength(1)
                    ))
                );
                song_results = songs.Documents.Cast<Subject>();
            }
            return person_results.Union(artist_results).Union(song_results).ToList();
        }

        public async Task<Tuple<int, int>> GetLikes(int subjectId)
        {
            var likes = mintplayer_context.Likes
                .Where(l => l.SubjectId == subjectId)
                .Where(l => l.DoesLike)
                .Count();
            var dislikes = mintplayer_context.Likes
                .Where(l => l.SubjectId == subjectId)
                .Where(l => !l.DoesLike)
                .Count();
            return new Tuple<int, int>(likes, dislikes);
        }

        public async Task<bool?> DoesLike(int subjectId)
        {
            var user = await user_manager.GetUserAsync(http_context.HttpContext.User);
            var user_id = user?.Id ?? 0;

            if (user_id == 0) throw new UnauthorizedAccessException();

            var like = mintplayer_context.Likes
                .Where(l => l.SubjectId == subjectId)
                .Where(l => l.UserId == user_id)
                .FirstOrDefault();

            if (like == null) return null;
            else return like.DoesLike;
        }

        public async Task Like(int subjectId, bool like)
        {
            var user = await user_manager.GetUserAsync(http_context.HttpContext.User);
            var subject_db = await mintplayer_context.FindAsync<Entities.Subject>(subjectId);

            var existing_like = mintplayer_context.Likes
                .Where(l => l.SubjectId == subjectId)
                .Where(l => l.UserId == user.Id)
                .FirstOrDefault();

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

        public async Task SaveChangesAsync()
        {
            await mintplayer_context.SaveChangesAsync();
        }
    }
}
