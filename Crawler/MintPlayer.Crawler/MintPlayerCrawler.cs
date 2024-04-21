using MintPlayer.Crawler.Events.EventArgs;
using MintPlayer.Crawler.Events.EventHandlers;
using MintPlayer.Fetcher.Abstractions;
using System.Diagnostics;

namespace MintPlayer.Crawler;

public class MintPlayerCrawler
{
	private readonly IFetcherContainer fetcherContainer;
	private readonly List<CrawlJob<Fetcher.Abstractions.Dtos.Subject>> crawlJobs;
	public MintPlayerCrawler(IFetcherContainer fetcherContainer)
	{
		this.fetcherContainer = fetcherContainer;
		crawlJobs = new List<CrawlJob<Fetcher.Abstractions.Dtos.Subject>>();
	}

	#region SubjectsDiscovered
	public event SubjectsDiscoveredEventHandler SubjectsDiscovered;
	protected void OnSubjectsDiscovered(SubjectsDiscoveredEventArgs e)
	{
		if (SubjectsDiscovered != null)
			SubjectsDiscovered(this, e);
	}
	#endregion

	public async Task Start()
	{
		var start_urls = new string[] {
			"https://genius.com/Dario-g-sunchyme-lyrics",
			"https://genius.com/The-weeknd-i-feel-it-coming-lyrics",
			"https://genius.com/artists/Daft-punk",
			"https://genius.com/albums/Daft-punk/Random-access-memories",
			"https://www.musixmatch.com/lyrics/Roddy-Ricch/The-Box",
			"https://www.musixmatch.com/lyrics/Daft-Punk-feat-Nile-Rodgers-Pharrell-Williams/Get-lucky",
			"https://www.musixmatch.com/lyrics/Ariana-Grande/Love-Me-Harder-feat-The-Weeknd",
			"https://www.musixmatch.com/artist/The-Weeknd",
			"https://www.musixmatch.com/album/The-Weeknd/The-Weeknd-In-Japan",
			"https://www.lyrics.com/lyric/29531449",
			"https://www.lyrics.com/artist/Daft-Punk/168791",
			"https://www.lyrics.com/album/3810277/100-Greatest-Classic-Rock-Songs",
			"https://www.lyrics.com/album/2860319/Life-is-But-a-Dream",
			"https://songteksten.net/lyric/1818/11910/france-gall/ella-elle-la.html",
			"https://songteksten.net/lyric/97/96025/daft-punk/get-lucky.html",
			"https://songteksten.net/albums/album/235/103/red-hot-chili-peppers/californication.html",
			"https://songteksten.net/artist/lyrics/97/daft-punk.html",
			"https://songteksten.net/artist/lyrics/235/red-hot-chili-peppers.html"
		};

		crawlJobs.AddRange(start_urls.Select(u => new CrawlJob<Fetcher.Abstractions.Dtos.Subject> { Url = u }));
		while (true)
		{
			// Fetch maximum 20 subjects currently in the list
			var current_jobs = crawlJobs.Where(j =>
			{
				if (j == null) Debugger.Break();
				return j.Status == Enums.ECrawlStatus.Pending;
			}).Take(20).ToArray();
			var crawled_subjects_bloated = await Task.WhenAll(current_jobs.Select(async (job) =>
			{
				var subject = await RunJob(job);
				return subject;
			}));
			var crawled_subjects_filtered = crawled_subjects_bloated.Where(s => s != null);

			// Invoke the event
			OnSubjectsDiscovered(new SubjectsDiscoveredEventArgs { Subjects = crawled_subjects_filtered });

			// Create jobs for related urls
			crawlJobs.AddRange(crawled_subjects_filtered
				.SelectMany(s =>
				{
					if (s == null) Debugger.Break();
					return s.RelatedUrls;
				})
				.Where(url => !string.IsNullOrEmpty(url))
				.Except(crawlJobs.Select(j =>
				{
					if (j == null) Debugger.Break();
					return j.Url;
				}))
				.Select(url => new CrawlJob<Fetcher.Abstractions.Dtos.Subject> { Url = url }));
		}
	}

	private async Task<Fetcher.Abstractions.Dtos.Subject> RunJob(CrawlJob<Fetcher.Abstractions.Dtos.Subject> job)
	{
		try
		{
			if (job == null) Debugger.Break();

			var fetcher = fetcherContainer.GetFetcher(job.Url);
			var subject = await fetcher.Fetch(job.Url, true);
			Console.WriteLine($"Fetched {job.Url}");
			job.Status = Enums.ECrawlStatus.Success;

			await Task.Delay(5);
			return subject;
		}
		catch (Exception ex)
		{
			if (job == null) Debugger.Break();

			Console.ForegroundColor = ConsoleColor.Red;
			Console.WriteLine($"Error while fetching {job.Url}: {ex.Message}");
			Console.ResetColor();
			job.Status = Enums.ECrawlStatus.Error;
			return null;
		}
	}
}
