using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MintPlayer.Data.Abstractions.Services;
using MintPlayer.Fetcher.Abstractions;

namespace MintPlayer.Fetcher.Integration.Services
{
	public interface IFetcherService
	{
		Task<Dtos.FetchResult> Fetch(string url, bool trimTrash);
	}
	internal class FetcherService : IFetcherService
	{
		private readonly IFetcherContainer fetcherContainer;
		private readonly ISubjectService subjectService;
		public FetcherService(IFetcherContainer fetcherContainer, ISubjectService subjectService)
		{
			this.fetcherContainer = fetcherContainer;
			this.subjectService = subjectService;
		}

		public async Task<Dtos.FetchResult> Fetch(string url, bool trimTrash)
		{
			var fetchedSubject = await fetcherContainer.Fetch(url, trimTrash);
			var allUrls = fetchedSubject.RelatedUrls.Union(new[] { url }).ToArray();
			var related = await subjectService.GetByMedium(allUrls);

			switch (fetchedSubject)
			{
				case Abstractions.Dtos.Artist fetchedArtist:
					return new Dtos.FetchResult<Dtos.FetchedArtist>
					{
						FetchedSubject = new Dtos.FetchedArtist
						{
							Url = fetchedArtist.Url,
							Name = fetchedArtist.Name,
							ImageUrl = fetchedArtist.ImageUrl,
							Songs = fetchedArtist.Songs.Select(s => new Dtos.FetchResult<Dtos.FetchedSong>
							{
								FetchedSubject = new Dtos.FetchedSong
								{
									Url = s.Url,
									Title = s.Title,
									Released = s.ReleaseDate,
									Lyrics = s.Lyrics,
								}
							}).ToList(),
							Albums = fetchedArtist.Albums.Select(a => new Dtos.FetchResult<Dtos.FetchedAlbum>
							{
								FetchedSubject = new Dtos.FetchedAlbum
								{
									Url = a.Url,
									Name = a.Name,
									ReleaseDate = a.ReleaseDate,
									CoverArtUrl = a.CoverArtUrl,
								}
							}).ToList(),
						},
						Candidates = new List<Dtos.SubjectWithCertainty<MintPlayer.Dtos.Dtos.Subject>>()
					};
				case Fetcher.Abstractions.Dtos.Album fetchedAlbum:
					throw new NotImplementedException();
				case Fetcher.Abstractions.Dtos.Song fetchedSong:
					var mediaLookup = await subjectService.GetByMedium(
						(new[] { fetchedSong.PrimaryArtist }).Union(fetchedSong.FeaturedArtists ?? new List<Abstractions.Dtos.Artist>())
							.Select(a => a.Url)
							.ToArray()
					);
					return new Dtos.FetchResult<Dtos.FetchedSong>
					{
						FetchedSubject = new Dtos.FetchedSong
						{
							Url = fetchedSong.Url,
							Title = fetchedSong.Title,
							Lyrics = fetchedSong.Lyrics,
							Released = fetchedSong.ReleaseDate,
							Artists = (new[] { fetchedSong.PrimaryArtist }).Union(fetchedSong.FeaturedArtists)
								.Select(a =>
								{
									return new Dtos.FetchResult<Dtos.FetchedArtist>
									{
										FetchedSubject = new Dtos.FetchedArtist
										{
											Url = a.Url,
											Name = a.Name,
											ImageUrl = a.ImageUrl,
										},
										Candidates = mediaLookup[a.Url]
											.Select(a => new Dtos.SubjectWithCertainty<MintPlayer.Dtos.Dtos.Subject>
											{
												Subject = a,
												Certainty = Enums.ECertainty.Certain,
											}).ToList()
									};
								}).ToList()
						}
					};
				default:
					throw new NotImplementedException();
			}
		}
	}
}
