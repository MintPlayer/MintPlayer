using MintPlayer.Domain.Entities;
using MintPlayer.Web.Projections;
using Raven.Client.Documents.Indexes;

namespace MintPlayer.Web.Indexes;

/// <summary>
/// Aggregate like/dislike counts per subject. Because likes are stored per user (a <see cref="UserLike"/>
/// document with two id arrays), per-subject totals need a fan-out map-reduce: each array element emits
/// one row, and the reduce sums them by subject id. Two maps over the same collection (one for likes, one
/// for dislikes) feed a single reduce. Auto-registered at startup (scanned from this assembly).
///
/// <para>Like any RavenDB index this is eventually consistent — a count read straight after a like may lag
/// by milliseconds; the like endpoint waits for non-stale results so the returned totals are fresh.</para>
/// </summary>
public class Likes_Count : AbstractMultiMapIndexCreationTask<LikeCount>
{
    public Likes_Count()
    {
        AddMap<UserLike>(userLikes => from u in userLikes
                                      from subjectId in u.Likes
                                      select new LikeCount { SubjectId = subjectId, Likes = 1, Dislikes = 0 });

        AddMap<UserLike>(userLikes => from u in userLikes
                                      from subjectId in u.Dislikes
                                      select new LikeCount { SubjectId = subjectId, Likes = 0, Dislikes = 1 });

        Reduce = results => from r in results
                            group r by r.SubjectId into g
                            select new LikeCount
                            {
                                SubjectId = g.Key,
                                Likes = g.Sum(x => x.Likes),
                                Dislikes = g.Sum(x => x.Dislikes),
                            };
    }
}
