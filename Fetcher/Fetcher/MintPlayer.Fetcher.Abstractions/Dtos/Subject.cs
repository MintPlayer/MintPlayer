using System.Collections.Generic;

namespace MintPlayer.Fetcher.Abstractions.Dtos
{
    public abstract class Subject
    {
        public abstract IEnumerable<string> RelatedUrls { get; }
    }
}
