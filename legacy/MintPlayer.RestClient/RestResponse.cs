using System.Net;
using System.Net.Http.Headers;

namespace MintPlayer.RestClient
{
    public class RestResponse<TResponse>
    {
        internal RestResponse()
        {
        }

        public TResponse Data { get; internal set; }
        public HttpStatusCode StatusCode { get; set; }
        public HttpResponseHeaders Headers { get; set; }
    }
}
