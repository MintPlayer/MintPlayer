using MintPlayer.Spark.Abstractions;

namespace MintPlayer.Domain.Entities;

/// <summary>
/// A user-curated, ordered collection of songs (legacy <c>Playlist</c>). Display = <c>[Breadcrumb]</c> "{Name}".
/// Tracks are embedded as <see cref="Tracks"/> (AsDetail), ordered by <see cref="PlaylistTrack.Index"/>.
///
/// Ownership + visibility drive row-level security (see <c>PlaylistActions.IsAllowedAsync</c>):
/// <see cref="OwnerId"/> is stamped with the creating user on first save; <see cref="IsPublic"/> decides
/// whether anyone (else) may read it. Owners read/edit/delete their own; everyone may read public ones.
/// </summary>
[Breadcrumb("{Name}")]
public class Playlist : Entity
{
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    /// <summary>When true, the playlist is readable by everyone; otherwise only by its owner.</summary>
    public bool IsPublic { get; set; }

    /// <summary>
    /// <c>SparkUser</c> id of the owner, stamped on create and preserved on edit. Drives row-level
    /// security; hidden + read-only in the auto-UI (it is set server-side, never by the editor).
    /// </summary>
    public string? OwnerId { get; set; }

    /// <summary>
    /// The ordered tracks. Order is the array position itself (drag-reorder via Spark's
    /// <c>[Sortable]</c> AsDetail support — no explicit index field needed).
    /// </summary>
    [Sortable]
    public List<PlaylistTrack> Tracks { get; set; } = [];
}
