using System.Collections.Generic;

namespace MintPlayer.Fetcher.Dtos
{
    public abstract class Subject
    {
        public abstract IEnumerable<string> RelatedUrls { get; }
    }
}
