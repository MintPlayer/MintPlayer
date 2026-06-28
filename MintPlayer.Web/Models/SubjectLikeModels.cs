namespace MintPlayer.Web.Models;

/// <summary>
/// The like state of a subject for the current request: public totals plus, when the caller is signed in,
/// their own preference. Mirrors the legacy <c>SubjectLikeResult</c>.
/// </summary>
public class SubjectLikeResult
{
    /// <summary>Number of users who like this subject.</summary>
    public int Likes { get; set; }

    /// <summary>Number of users who dislike this subject.</summary>
    public int Dislikes { get; set; }

    /// <summary>The current user's preference: <c>true</c> = likes, <c>false</c> = dislikes, <c>null</c> = neither.</summary>
    public bool? Like { get; set; }

    /// <summary>Whether the caller is authenticated (so the UI knows if <see cref="Like"/> is meaningful).</summary>
    public bool Authenticated { get; set; }
}

/// <summary>
/// Body of <c>POST /api/subject/likes</c>: set the current user's preference for a subject.
/// <see cref="Like"/> = <c>true</c> likes, <c>false</c> dislikes, <c>null</c> clears (unlike).
/// </summary>
public class SetLikeRequest
{
    public string SubjectId { get; set; } = string.Empty;
    public bool? Like { get; set; }
}
