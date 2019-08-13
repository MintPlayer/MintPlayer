using System.Net.Http;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using System.Security.Claims;
using System.Net.Http.Headers;
using System.Text.Encodings.Web;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OAuth;

namespace Identity.ExternalProviders.LinkedIn
{
    internal class LinkedinHandler : OAuthHandler<LinkedInOptions>
    {
        internal LinkedinHandler(IOptionsMonitor<LinkedInOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock)
            : base(options, logger, encoder, clock)
        {
        }

        protected override async Task<AuthenticationTicket> CreateTicketAsync(ClaimsIdentity identity, AuthenticationProperties properties, OAuthTokenResponse tokens)
        {
            // Retrieve user info
            var request = new HttpRequestMessage(HttpMethod.Get, this.Options.UserInformationEndpoint);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokens.AccessToken);
            request.Headers.Add("x-li-format", "json");
            var response = await this.Backchannel.SendAsync(request, this.Context.RequestAborted);
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            var user = JObject.Parse(content);
            OAuthCreatingTicketContext context = new OAuthCreatingTicketContext(new ClaimsPrincipal(identity), properties, this.Context, this.Scheme, this.Options, this.Backchannel, tokens, user);
            context.RunClaimActions();
            await this.Events.CreatingTicket(context);
            return new AuthenticationTicket(context.Principal, context.Properties, this.Scheme.Name);
        }
    }
}
