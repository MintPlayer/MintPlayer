namespace MintPlayer.Web.Projections;

/// <summary>
/// Per-subject like/dislike totals, the reduce result of the <see cref="MintPlayer.Web.Indexes.Likes_Count"/>
/// fan-out map-reduce index over <c>UserLike</c> documents. One row per subject id that anyone has
/// liked or disliked.
/// </summary>
public class LikeCount
{
    public string SubjectId { get; set; } = string.Empty;
    public int Likes { get; set; }
    public int Dislikes { get; set; }
}
