using System.Collections.Generic;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MintPlayer.Fetcher
{
    public abstract class Fetcher : IFetcher
    {
        public abstract IEnumerable<Regex> UrlRegex { get; }
        public abstract Task<Dtos.Subject> Fetch(string url, bool trimTrash);

        protected async Task<string> SendRequest(HttpClient httpClient, string url)
        {
            var response = await httpClient.GetAsync(url);
            var html = await response.Content.ReadAsStringAsync();
            return html;
        }
    }
}
