using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using MintPlayer.Fetcher.Exceptions;

[assembly: InternalsVisibleTo("MintPlayer.Fetcher.Test")]
namespace MintPlayer.Fetcher
{
    internal class FetcherContainer : Abstractions.IFetcherContainer
	{
        public FetcherContainer(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        private IServiceProvider serviceProvider;

        public Abstractions.IFetcher GetFetcher(string url)
        {
            var fetchers = serviceProvider.GetServices<Abstractions.IFetcher>();
            var fetcher = fetchers.FirstOrDefault((f) => f.UrlRegex.Any(rgx => rgx.IsMatch(url)));

            if (fetcher == null)
                throw new NoFetcherFoundException(url);

            return fetcher;
        }

        public async Task<Abstractions.Dtos.Subject> Fetch(string url, bool trimTrash)
        {
            var subject = await GetFetcher(url).Fetch(url, trimTrash);
            return subject;
        }
    }
}
