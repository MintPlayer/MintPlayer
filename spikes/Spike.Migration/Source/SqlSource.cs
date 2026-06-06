namespace Spike.Migration.Source;

// POCO mirror of the current SQL Server / EF Core rows the real ETL (Phase 7) will read
// out of MintPlayer.Data.MintPlayerContext. The production entities are `internal`, so for
// this spike we hand-seed equivalent rows rather than reference the EF context — the point
// here is to prove the *target* shape and the auth round-trips, not the EF read itself.

public sealed class SqlArtist
{
    public int Id { get; init; }
    public string Name { get; init; } = "";
    public int? YearStarted { get; init; }
    public int? YearQuit { get; init; }
}

public sealed class SqlPerson
{
    public int Id { get; init; }
    public string FirstName { get; init; } = "";
    public string LastName { get; init; } = "";
    public DateTime? Born { get; init; }
}

public sealed class SqlSong
{
    public int Id { get; init; }
    public string Title { get; init; } = "";
    public DateTime Released { get; init; }
}

// Join rows carrying business flags (ArtistPerson.Active, ArtistSong.Credited, PlaylistSong.Index).
public sealed class SqlArtistPerson
{
    public int ArtistId { get; init; }
    public int PersonId { get; init; }
    public bool Active { get; init; }
}

public sealed class SqlArtistSong
{
    public int ArtistId { get; init; }
    public int SongId { get; init; }
    public bool Credited { get; init; }
}

public sealed class SqlTagCategory
{
    public int Id { get; init; }
    public int ColorArgb { get; init; } // EF stored System.Drawing.Color as ToArgb()
    public string Description { get; init; } = "";
}

public sealed class SqlTag
{
    public int Id { get; init; }
    public string Description { get; init; } = "";
    public int CategoryId { get; init; }
    public int? ParentId { get; init; } // self-tree
}

public sealed class SqlSubjectTag
{
    public int SubjectId { get; init; }
    public int TagId { get; init; }
}

public sealed class SqlMedium
{
    public int SubjectId { get; init; }
    public string Type { get; init; } = "";
    public string Value { get; init; } = "";
}

/// <summary>A small but representative snapshot of the source database.</summary>
public static class SqlSnapshot
{
    public static readonly SqlArtist[] Artists =
    [
        new() { Id = 1, Name = "Daft Punk", YearStarted = 1993, YearQuit = 2021 },
    ];

    public static readonly SqlPerson[] People =
    [
        new() { Id = 3, FirstName = "Thomas", LastName = "Bangalter", Born = new DateTime(1975, 1, 3) },
        new() { Id = 4, FirstName = "Guy-Manuel", LastName = "de Homem-Christo", Born = new DateTime(1974, 2, 8) },
    ];

    public static readonly SqlSong[] Songs =
    [
        new() { Id = 10, Title = "One More Time", Released = new DateTime(2000, 11, 13) },
        new() { Id = 11, Title = "Harder, Better, Faster, Stronger", Released = new DateTime(2001, 10, 13) },
    ];

    public static readonly SqlArtistPerson[] ArtistPeople =
    [
        new() { ArtistId = 1, PersonId = 3, Active = true },
        new() { ArtistId = 1, PersonId = 4, Active = false }, // left the band — flag must survive
    ];

    public static readonly SqlArtistSong[] ArtistSongs =
    [
        new() { ArtistId = 1, SongId = 10, Credited = true },
        new() { ArtistId = 1, SongId = 11, Credited = false }, // uncredited — flag must survive
    ];

    public static readonly SqlTagCategory[] TagCategories =
    [
        new() { Id = 1, ColorArgb = unchecked((int)0xFF1E90FF), Description = "Genre" },
    ];

    public static readonly SqlTag[] Tags =
    [
        new() { Id = 1, Description = "Electronic", CategoryId = 1, ParentId = null },
        new() { Id = 2, Description = "House", CategoryId = 1, ParentId = 1 }, // child of Electronic
    ];

    public static readonly SqlSubjectTag[] SubjectTags =
    [
        new() { SubjectId = 1, TagId = 1 },
        new() { SubjectId = 1, TagId = 2 },
    ];

    public static readonly SqlMedium[] Media =
    [
        new() { SubjectId = 1, Type = "Spotify", Value = "4tZwfgrHOc3mvqYlEYSvVi" },
    ];
}
