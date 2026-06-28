using MintPlayer.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MintPlayer.Data.Helpers
{
    internal class SubjectHelper
    {
        internal void CalculateUpdatedTags(Subject old, MintPlayer.Dtos.Dtos.Subject @new, MintPlayerContext mintplayer_context, out IEnumerable<SubjectTag> to_add, out IEnumerable<SubjectTag> to_remove)
        {
            to_remove = old.Tags
                .Where(st => !@new.Tags.Select(t => t.Id).Contains(st.TagId))
                .ToArray();
            to_add = @new.Tags
                .Where(t => !old.Tags.Select(st => st.TagId).Contains(t.Id))
                .Select(t => new SubjectTag(old, mintplayer_context.Tags.Find(t.Id)))
                .ToArray();
        }
    }
}
