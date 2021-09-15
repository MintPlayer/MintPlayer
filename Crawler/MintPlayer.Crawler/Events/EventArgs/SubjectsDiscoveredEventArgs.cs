using MintPlayer.Fetcher.Abstractions.Dtos;
using System.Collections.Generic;

namespace MintPlayer.Crawler.Events.EventArgs
{
    public class SubjectsDiscoveredEventArgs : System.EventArgs
    {
        public SubjectsDiscoveredEventArgs(IEnumerable<Subject> subjects)
        {
            Subjects = subjects;
        }

        public IEnumerable<Subject> Subjects { get; private set; }
    }
}
