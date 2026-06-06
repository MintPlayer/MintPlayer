using MintPlayer.Spark.Testing;
using Raven.Client.Documents;
using Spike.Migration.Domain;
using Spike.Migration.Etl;

namespace Spike.Migration;

/// <summary>
/// Proves the structural migration of the polymorphic SQL <c>Subject</c> TPH table + its join
/// tables into RavenDB collections: the three subject types split into separate collections,
/// join-table business flags (ArtistPerson.Active, ArtistSong.Credited) survive as embedded
/// arrays, cross-collection [Reference] string IDs resolve to real documents, the Tag self-tree
/// resolves, the TagCategory ARGB colour round-trips, and the deterministic IDs make the ETL
/// idempotent.
/// </summary>
public class ReferenceResolutionTests : SparkTestDriver
{
    [Fact]
    public async Task Catalog_migrates_with_resolvable_references_and_preserved_flags()
    {
        await Migrator.MigrateCatalogAsync(Store);

        using var session = Store.OpenAsyncSession();

        var artist = await session.LoadAsync<Artist>("artists/1");
        artist.Should().NotBeNull();
        artist!.Name.Should().Be("Daft Punk");
        artist.OldId.Should().Be(1);
        artist.Media.Should().ContainSingle(m => m.Type == "Spotify");

        // --- Join-table flags survived the fold into embedded arrays ---
        artist.Members.Should().HaveCount(2);
        artist.Members.Single(m => m.PersonId == "people/3").Active.Should().BeTrue();
        artist.Members.Single(m => m.PersonId == "people/4").Active.Should().BeFalse();

        artist.Tracks.Single(t => t.SongId == "songs/10").Credited.Should().BeTrue();
        artist.Tracks.Single(t => t.SongId == "songs/11").Credited.Should().BeFalse();

        // --- Cross-collection references resolve (TPH split into separate collections) ---
        var thomas = await session.LoadAsync<Person>("people/3");
        thomas!.LastName.Should().Be("Bangalter");

        var song = await session.LoadAsync<Song>("songs/10");
        song!.Title.Should().Be("One More Time");

        // Bulk reference resolution in a single round-trip (how Spark resolves breadcrumbs).
        var people = await session.LoadAsync<Person>(artist.Members.Select(m => m.PersonId));
        people.Values.Should().OnlyContain(p => p != null);
        people.Should().HaveCount(2);

        // --- Tag self-tree ---
        var house = await session.LoadAsync<Tag>("tags/2");
        house!.ParentId.Should().Be("tags/1");
        var electronic = await session.LoadAsync<Tag>(house.ParentId!);
        electronic!.Description.Should().Be("Electronic");

        // --- TagCategory ARGB colour round-trip ---
        var category = await session.LoadAsync<TagCategory>("tagcategories/1");
        category!.ColorArgb.Should().Be(unchecked((int)0xFF1E90FF));
    }

    [Fact]
    public async Task Migration_is_idempotent_via_deterministic_ids()
    {
        await Migrator.MigrateCatalogAsync(Store);
        await Migrator.MigrateCatalogAsync(Store); // re-run against the same store

        using var session = Store.OpenAsyncSession();

        // Deterministic IDs overwrite rather than append: still exactly one artists/1 with 2 members.
        var artist = await session.LoadAsync<Artist>("artists/1");
        artist!.Members.Should().HaveCount(2);

        var artistCount = await session.Query<Artist>()
            .Customize(x => x.WaitForNonStaleResults())
            .CountAsync();
        artistCount.Should().Be(1);
    }
}
