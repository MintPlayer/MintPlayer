using MintPlayer.Spark.Abstractions;
using Raven.Client.Documents;
using Spike.Migration.Domain;
using Spike.Migration.Source;

namespace Spike.Migration.Etl;

/// <summary>
/// Maps a SQL snapshot into the Spark/RavenDB domain and writes it with deterministic IDs.
/// The real Phase-7 tool will read from MintPlayer.Data via EF and write via BulkInsert; this
/// spike uses an ordinary async session against the embedded store, which is plenty for proving
/// the shape and reference resolution.
/// </summary>
public static class Migrator
{
    public static async Task MigrateCatalogAsync(IDocumentStore store, CancellationToken ct = default)
    {
        using var session = store.OpenAsyncSession();

        // People
        foreach (var p in SqlSnapshot.People)
        {
            await session.StoreAsync(new Person
            {
                Id = SparkIds.Person(p.Id),
                OldId = p.Id,
                FirstName = p.FirstName,
                LastName = p.LastName,
                Born = p.Born,
            }, ct);
        }

        // Songs
        foreach (var s in SqlSnapshot.Songs)
        {
            await session.StoreAsync(new Song
            {
                Id = SparkIds.Song(s.Id),
                OldId = s.Id,
                Title = s.Title,
                Released = s.Released,
            }, ct);
        }

        // Tag categories + tags (self-tree)
        foreach (var tc in SqlSnapshot.TagCategories)
        {
            await session.StoreAsync(new TagCategory
            {
                Id = SparkIds.TagCategory(tc.Id),
                OldId = tc.Id,
                ColorArgb = tc.ColorArgb,
                Description = tc.Description,
            }, ct);
        }

        foreach (var t in SqlSnapshot.Tags)
        {
            await session.StoreAsync(new Tag
            {
                Id = SparkIds.Tag(t.Id),
                OldId = t.Id,
                Description = t.Description,
                CategoryId = SparkIds.TagCategory(t.CategoryId),
                ParentId = t.ParentId is int parent ? SparkIds.Tag(parent) : null,
            }, ct);
        }

        // Artists — fold the join tables into embedded arrays, preserving their flags
        foreach (var a in SqlSnapshot.Artists)
        {
            var members = SqlSnapshot.ArtistPeople
                .Where(ap => ap.ArtistId == a.Id)
                .Select(ap => new ArtistMember { PersonId = SparkIds.Person(ap.PersonId), Active = ap.Active })
                .ToList();

            var tracks = SqlSnapshot.ArtistSongs
                .Where(asg => asg.ArtistId == a.Id)
                .Select(asg => new ArtistTrack { SongId = SparkIds.Song(asg.SongId), Credited = asg.Credited })
                .ToList();

            var tags = SqlSnapshot.SubjectTags
                .Where(st => st.SubjectId == a.Id)
                .Select(st => SparkIds.Tag(st.TagId))
                .ToList();

            var media = SqlSnapshot.Media
                .Where(m => m.SubjectId == a.Id)
                .Select(m => new Medium { Type = m.Type, Value = m.Value })
                .ToList();

            await session.StoreAsync(new Artist
            {
                Id = SparkIds.Artist(a.Id),
                OldId = a.Id,
                Name = a.Name,
                YearStarted = a.YearStarted,
                YearQuit = a.YearQuit,
                Members = members,
                Tracks = tracks,
                Tags = tags,
                Media = media,
            }, ct);
        }

        await session.SaveChangesAsync(ct);
    }
}
