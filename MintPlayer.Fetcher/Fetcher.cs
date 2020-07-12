using System.Collections.Generic;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MintPlayer.Fetcher
{
    public interface IFetcher
    {
        IEnumerable<Regex> UrlRegex { get; }
        Task<Dtos.Subject> Fetch(string url, bool trimTrash);
    }

    public abstract class Fetcher : IFetcher
    {
        public abstract IEnumerable<Regex> UrlRegex { get; }
        public abstract Task<Dtos.Subject> Fetch(string url, bool trimTrash);
    }
}
