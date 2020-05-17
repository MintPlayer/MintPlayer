using Microsoft.Extensions.Options;
using MintPlayer.RestClient.Enums;
using MintPlayer.RestClient.Options;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace MintPlayer.RestClient
{
    public interface IRestClient
    {
        Task Get<TResponse>(string url, Action<RestResponse<TResponse>> success, Action error);
        HttpRequestHeaders AdditionalHeaders { get; }
    }

    internal class RestClient : IRestClient
    {
        private readonly HttpClient httpClient;
        private readonly IOptions<RestClientOptions> restClientOptions;
        public RestClient(HttpClient httpClient, IOptions<RestClientOptions> restClientOptions)
        {
            this.httpClient = httpClient;
            this.restClientOptions = restClientOptions;
        }

        private eMimeType GetMimeType(string contentType)
        {
            switch (contentType)
            {
                case "text/plain":
                    return eMimeType.Text;
                case "text/xml":
                case "application/xml":
                    return eMimeType.Xml;
                case "application/json":
                    return eMimeType.Json;
                default:
                    throw new Exception("Content-Type not recognized");
            }
        }
        private string UriCombine(params string[] segments)
        {
            return string.Join("/", segments.Select(s => s.Trim('/')));
        }
        private async Task<RestResponse<TResponse>> ProcessResponse<TResponse>(HttpResponseMessage response)
        {
            return new RestResponse<TResponse>
            {
                Data = await ProcessContent<TResponse>(response),
                Headers = response.Headers,
                StatusCode = response.StatusCode
            };
        }
        private async Task<TResponse> ProcessContent<TResponse>(HttpResponseMessage message)
        {
            var content = await message.Content.ReadAsStringAsync();
            var mimeType = GetMimeType(message.Content.Headers.ContentType.MediaType);

            switch (mimeType)
            {
                case eMimeType.Text:
                    // int, double, string, float
                    return (TResponse)Convert.ChangeType(content, typeof(TResponse));
                case eMimeType.Xml:
                    var serializer = new XmlSerializer(typeof(TResponse));
                    var result = (TResponse)serializer.Deserialize(new System.IO.StringReader(content));
                    return result;
                case eMimeType.Json:
                    return JsonConvert.DeserializeObject<TResponse>(content);
                default:
                    throw new Exception("Invalid Content-Type");
            }
        }

        public async Task Get<TResponse>(string url, Action<RestResponse<TResponse>> success, Action error)
        {
            var path = UriCombine(restClientOptions.Value.BaseUrl, url);
            var response = await httpClient.GetAsync(path);

            var result = await ProcessResponse<TResponse>(response);
            success(result);
        }

        public HttpRequestHeaders AdditionalHeaders => httpClient.DefaultRequestHeaders;
    }
}
