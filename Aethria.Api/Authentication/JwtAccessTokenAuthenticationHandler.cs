using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Text.Encodings.Web;

namespace Aethria.Api.Authentication;

internal sealed class JwtAccessTokenAuthenticationHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder,
    IConfiguration configuration) : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    public const string SchemeName = "Bearer";

    private readonly string _issuer =
        configuration["Auth:Issuer"]
        ?? throw new InvalidOperationException("Missing required configuration value 'Auth:Issuer'.");

    private readonly string _audience =
        configuration["Auth:Audience"]
        ?? throw new InvalidOperationException("Missing required configuration value 'Auth:Audience'.");

    private readonly string _signingKey =
        configuration["Auth:SigningKey"]
        ?? throw new InvalidOperationException("Missing required configuration value 'Auth:SigningKey'.");

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var token = ResolveAccessToken();
        if (string.IsNullOrWhiteSpace(token))
        {
            return AuthenticateResult.NoResult();
        }

        try
        {
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = _issuer,
                ValidateAudience = true,
                ValidAudience = _audience,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_signingKey)),
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromMinutes(1),
                NameClaimType = ClaimTypes.NameIdentifier
            };

            var result = await new JsonWebTokenHandler().ValidateTokenAsync(token, validationParameters);
            if (!result.IsValid || result.ClaimsIdentity is null)
            {
                return AuthenticateResult.Fail("Invalid bearer token.");
            }

            var principal = new ClaimsPrincipal(result.ClaimsIdentity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);

            return AuthenticateResult.Success(ticket);
        }
        catch (Exception exception) when (exception is SecurityTokenException or ArgumentException)
        {
            return AuthenticateResult.Fail("Invalid bearer token.");
        }
    }

    private string? ResolveAccessToken()
    {
        var authorization = Request.Headers.Authorization.ToString();
        if (!string.IsNullOrWhiteSpace(authorization) && authorization.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            return authorization["Bearer ".Length..].Trim();
        }

        if (Request.Path.StartsWithSegments("/hubs", StringComparison.OrdinalIgnoreCase)
            && Request.Query.TryGetValue("access_token", out var accessToken))
        {
            return accessToken.ToString();
        }

        return null;
    }
}
