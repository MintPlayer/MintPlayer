using MintPlayer.Fetcher.Abstractions.Dtos;

namespace MintPlayer.Crawler.Events.EventArgs;

public class SubjectsDiscoveredEventArgs : System.EventArgs
{
	public IEnumerable<Subject> Subjects { get; init; }
}
