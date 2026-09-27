namespace MintPlayer.Domain.Entities;

/// <summary>
/// Base type for every MintPlayer catalog entity stored in RavenDB. Centralises the
/// conventions established in implementation-plan step 1.4 so each concrete entity only
/// carries its own domain fields:
/// <list type="bullet">
///   <item><b>Audit timestamps</b> — <see cref="CreatedAt"/> / <see cref="ModifiedAt"/>,
///   stamped server-side by <c>EntityActions&lt;T&gt;</c> on save.</item>
///   <item><b>Soft delete</b> — <see cref="IsDeleted"/> / <see cref="DeletedAt"/>. Deleting a
///   row flags it rather than removing it; it is then filtered out of every query, single
///   load, and the named-query list path, but kept for audit/restore.</item>
///   <item><b>Migration anchor</b> — <see cref="OldId"/>, the integer primary key from the
///   legacy SQL Server database, retained so the data migration can resolve cross-references
///   and stay idempotent across re-runs.</item>
/// </list>
/// All five fields are managed by the framework/Actions layer and hidden + read-only in the
/// auto-UI (see the generated <c>App_Data/Model/*.json</c>).
/// </summary>
public abstract class Entity
{
    /// <summary>RavenDB document id (e.g. <c>mediumtypes/1-A</c>); null before the first store.</summary>
    public string? Id { get; set; }

    /// <summary>UTC instant the row was first stored. Set once on create, never updated.</summary>
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>UTC instant of the most recent edit; null until the row is updated.</summary>
    public DateTimeOffset? ModifiedAt { get; set; }

    /// <summary>Soft-delete flag. Deleted rows are filtered out everywhere but never physically removed.</summary>
    public bool IsDeleted { get; set; }

    /// <summary>UTC instant the row was soft-deleted; null while the row is live.</summary>
    public DateTimeOffset? DeletedAt { get; set; }

    /// <summary>Primary key from the legacy SQL Server database; null for rows created post-migration.</summary>
    public int? OldId { get; set; }
}
