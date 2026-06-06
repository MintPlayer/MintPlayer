using MintPlayer.Spark.Abstractions;

namespace Spike.Migration.Domain;

// Target RavenDB / Spark domain. The polymorphic SQL `Subject` TPH table collapses into three
// top-level collections (Artist / Person / Song). Join tables become embedded arrays that keep
// their business flag. References between collections are plain string document IDs, annotated
// with [Reference] so Spark's metadata UI can resolve them (RavenDB stores only the id string).
//
// Deterministic IDs derived from the old PKs (artists/1, people/3, songs/10) make the ETL
// idempotent and make reference resolution trivial. OldId is kept for traceability.

public sealed class Artist
{
    public string? Id { get; set; } // "artists/1"
    public int OldId { get; set; }
    public string Name { get; set; } = "";
    public int? YearStarted { get; set; }
    public int? YearQuit { get; set; }

    public List<ArtistMember> Members { get; set; } = []; // was ArtistPerson (AsDetail)
    public List<ArtistTrack> Tracks { get; set; } = [];   // was ArtistSong  (AsDetail)

    [Reference(typeof(Tag))]
    public List<string> Tags { get; set; } = [];          // was SubjectTag

    public List<Medium> Media { get; set; } = [];
}

public sealed class ArtistMember
{
    [Reference(typeof(Person))]
    public string PersonId { get; set; } = ""; // "people/3"
    public bool Active { get; set; }            // ArtistPerson.Active — must survive the migration
}

public sealed class ArtistTrack
{
    [Reference(typeof(Song))]
    public string SongId { get; set; } = ""; // "songs/10"
    public bool Credited { get; set; }        // ArtistSong.Credited — must survive the migration
}

public sealed class Medium
{
    public string Type { get; set; } = "";
    public string Value { get; set; } = "";
}

public sealed class Person
{
    public string? Id { get; set; } // "people/3"
    public int OldId { get; set; }
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public DateTime? Born { get; set; }
}

public sealed class Song
{
    public string? Id { get; set; } // "songs/10"
    public int OldId { get; set; }
    public string Title { get; set; } = "";
    public DateTime Released { get; set; }
}

public sealed class TagCategory
{
    public string? Id { get; set; } // "tagcategories/1"
    public int OldId { get; set; }
    public int ColorArgb { get; set; } // System.Drawing.Color.ToArgb() copied verbatim
    public string Description { get; set; } = "";
}

public sealed class Tag
{
    public string? Id { get; set; } // "tags/1"
    public int OldId { get; set; }
    public string Description { get; set; } = "";

    [Reference(typeof(TagCategory))]
    public string CategoryId { get; set; } = "";

    [Reference(typeof(Tag))]
    public string? ParentId { get; set; } // self-tree
}

public static class SparkIds
{
    public static string Artist(int oldId) => $"artists/{oldId}";
    public static string Person(int oldId) => $"people/{oldId}";
    public static string Song(int oldId) => $"songs/{oldId}";
    public static string Tag(int oldId) => $"tags/{oldId}";
    public static string TagCategory(int oldId) => $"tagcategories/{oldId}";
}
