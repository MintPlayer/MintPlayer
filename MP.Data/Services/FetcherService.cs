using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using MintPlayer.Fetcher.Abstractions;

namespace MintPlayer.Data.Services
{
	public interface IFetcherService
	{
		Task<Fetcher.Abstractions.Dtos.Subject> Fetch(string url, bool trimTrash);
	}
	internal class FetcherService : IFetcherService
	{
		private readonly IFetcherContainer fetcherContainer;
		public FetcherService(IFetcherContainer fetcherContainer)
		{
			this.fetcherContainer = fetcherContainer;
		}

		public async Task<Fetcher.Abstractions.Dtos.Subject> Fetch(string url, bool trimTrash)
		{
			var subject = await fetcherContainer.Fetch(url, trimTrash);
			return subject;
		}
	}
}
