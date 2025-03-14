using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Linq;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace API.Security;
public class ApiKeyAuthenticationHandler : AuthenticationHandler<ApiKeyAuthenticationOptions>
{
    public ApiKeyAuthenticationHandler(
        IOptionsMonitor<ApiKeyAuthenticationOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder)
    {
        
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue(Options.ApiKeyHeaderName, out var apiKeyHeaderValues))
        {
            return AuthenticateResult.Fail("Unauthorized");
        }

        var providedApiKey = apiKeyHeaderValues.FirstOrDefault();

        if (apiKeyHeaderValues.Count == 0 || string.IsNullOrWhiteSpace(providedApiKey))
        {
            return AuthenticateResult.Fail("Unauthorized");
        }

        var configuredApiKeys = await Task.Run(() => Context.RequestServices.GetRequiredService<IConfiguration>().GetSection("ApiKeys").Get<string[]>());

        if (configuredApiKeys != null && configuredApiKeys.Contains(providedApiKey))
        {
            var claims = new[] { new System.Security.Claims.Claim("ApiKey", providedApiKey) };
            var identity = new System.Security.Claims.ClaimsIdentity(claims, Options.Scheme);
            var identities = new[] { identity };
            var principal = new System.Security.Claims.ClaimsPrincipal(identities);
            var ticket = new AuthenticationTicket(principal, Options.Scheme);

            return AuthenticateResult.Success(ticket);
        }

        return AuthenticateResult.Fail("Unauthorized");
    }
}

