using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using MintPlayer.Fetcher.Exceptions;

namespace MintPlayer.Fetcher
{
    public interface IFetcherContainer
    {
        IFetcher GetFetcher(string url);
        Task<Dtos.Subject> Fetch(string url, bool trimTrash);
    }

    internal class FetcherContainer : IFetcherContainer
    {
        public FetcherContainer(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        private IServiceProvider serviceProvider;

        public IFetcher GetFetcher(string url)
        {
            var fetchers = serviceProvider.GetServices<IFetcher>();
            var fetcher = fetchers.FirstOrDefault((f) => f.UrlRegex.Any(rgx => rgx.IsMatch(url)));

            if (fetcher == null)
                throw new NoFetcherFoundException(url);

            return fetcher;
        }

        public Task<Dtos.Subject> Fetch(string url, bool trimTrash)
        {
            return GetFetcher(url).Fetch(url, trimTrash);
        }
    }
}
