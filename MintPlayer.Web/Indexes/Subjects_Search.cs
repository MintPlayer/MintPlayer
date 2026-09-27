using MintPlayer.Domain.Entities;
using MintPlayer.Web.Projections;
using Raven.Client.Documents.Indexes;

namespace MintPlayer.Web.Indexes;

/// <summary>
/// Cross-collection full-text search index over the catalog: maps <see cref="Artist"/>,
/// <see cref="Person"/> and <see cref="Song"/> into the shared <see cref="VSubject"/> shape so one
/// query (and one autocomplete) spans the whole catalog — the RavenDB-native replacement for the
/// legacy Elasticsearch index (D1). Soft-deleted rows are excluded at index time.
///
/// <para>The <c>Text</c> field is full-text indexed (<see cref="FieldIndexing.Search"/>) and has
/// suggestions enabled, so the <c>SearchController</c> can do both <c>.Search()</c> and
/// <c>.SuggestUsing()</c> against it. Auto-registered at startup (scanned from this assembly).</para>
/// </summary>
public class Subjects_Search : AbstractMultiMapIndexCreationTask<VSubject>
{
    public Subjects_Search()
    {
        AddMap<Artist>(artists => from artist in artists
                                  where !artist.IsDeleted
                                  select new VSubject
                                  {
                                      Id = artist.Id,
                                      SubjectType = "Artist",
                                      Text = artist.Name,
                                      TagIds = artist.TagIds,
                                  });

        AddMap<Person>(people => from person in people
                                 where !person.IsDeleted
                                 select new VSubject
                                 {
                                     Id = person.Id,
                                     SubjectType = "Person",
                                     Text = person.FirstName + " " + person.LastName,
                                     TagIds = person.TagIds,
                                 });

        AddMap<Song>(songs => from song in songs
                              where !song.IsDeleted
                              select new VSubject
                              {
                                  Id = song.Id,
                                  SubjectType = "Song",
                                  Text = song.Title,
                                  TagIds = song.TagIds,
                              });

        // Full-text on the display text; enable suggestions for the typo-tolerant autocomplete.
        Index(x => x.Text, FieldIndexing.Search);
        Suggestion(x => x.Text);

        // Store the projected fields so query results can be read straight from the index
        // (no document load), the same pattern as People_Overview.
        StoreAllFields(FieldStorage.Yes);
    }
}
