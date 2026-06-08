namespace MintPlayer.Domain.Entities;

/// <summary>
/// A single user's like/dislike state, as one document per user (id <c>UserLikes/{userId}</c>) holding
/// two arrays of subject ids. This is the RavenDB shape of the legacy per-(user,subject) <c>Like</c> rows
/// collapsed per user: it makes "my favorites" a single document load and toggling an atomic single-doc
/// write. A subject id can appear in at most one array (mutually exclusive like vs. dislike).
///
/// <para>Ids span all three catalog collections (<c>Artists</c>/<c>People</c>/<c>Songs</c>) — RavenDB ids
/// are globally unique, so one flat array per state covers every subject type. Per-subject totals
/// ("how many people like X") can't be read from here; they come from the <c>Likes_Count</c> fan-out
/// map-reduce index.</para>
/// </summary>
public class UserLike
{
    /// <summary>Document id — <c>UserLikes/{userId}</c> (deterministic, so the toggle is a load-or-create).</summary>
    public string? Id { get; set; }

    /// <summary>The owning user's id (the Identity / <c>SparkUser</c> id).</summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>Subject ids this user likes (<c>DoesLike = true</c> in the legacy model).</summary>
    public List<string> Likes { get; set; } = [];

    /// <summary>Subject ids this user dislikes (<c>DoesLike = false</c>).</summary>
    public List<string> Dislikes { get; set; } = [];
}
