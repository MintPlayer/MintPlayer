using System.Net.Http;
using System.Threading.Tasks;

namespace MintPlayer.Fetcher
{
    public interface IRequestSender
    {
        Task<string> SendRequest(string url);
    }
    internal class RequestSender : IRequestSender
    {
        private readonly HttpClient httpClient;
        public RequestSender(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        public async Task<string> SendRequest(string url)
        {
            var response = await httpClient.GetAsync(url);
            var html = await response.Content.ReadAsStringAsync();
            return html;
        }
    }
}
