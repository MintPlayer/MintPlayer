using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MintPlayer.Fetcher.Abstractions
{
    public interface IFetcher
    {
        IEnumerable<Regex> UrlRegex { get; }
        Task<Dtos.Subject> Fetch(string url, bool trimTrash);
    }
}
