using Aethria.Domain.Repositories;
using Aethria.Domain.ValueObjects;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;

namespace Aethria.McpServer.Authentication;

public sealed class ApiKeyAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public const string SchemeName = "ApiKey";
    private const string ApiKeyHeaderName = "X-Api-Key";

    private readonly IApiKeyRepository _apiKeyRepository;
    private readonly ILogger _logger;

    public ApiKeyAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory loggerFactory,
        UrlEncoder encoder,
        IApiKeyRepository apiKeyRepository)
        : base(options, loggerFactory, encoder)
    {
        _apiKeyRepository = apiKeyRepository;
        _logger = loggerFactory.CreateLogger<ApiKeyAuthenticationHandler>();
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        try
        {
            var headerValues = Request.Headers[ApiKeyHeaderName];
            var apiKeyValue = headerValues.FirstOrDefault();

            if (string.IsNullOrWhiteSpace(apiKeyValue))
            {
                return AuthenticateResult.NoResult();
            }

            var trimmedKey = apiKeyValue.Trim();
            var lastFour = trimmedKey.Length >= 4 ? trimmedKey[^4..] : "****";

            var hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(trimmedKey));
            var tokenHash = Convert.ToBase64String(hashBytes);

            var cancellationToken = Context.RequestAborted;

            var apiKey = await _apiKeyRepository.GetByTokenHashAsync(tokenHash, cancellationToken);
            if (apiKey is null)
            {
                _logger.LogWarning("API key authentication failed: no record found for key ending in ...{LastFour}", lastFour);
                return AuthenticateResult.Fail("Invalid or expired API key.");
            }

            if (apiKey.Status != ApiKeyStatus.Active)
            {
                _logger.LogWarning("API key authentication failed: key {KeyId} has status {Status}", apiKey.Id, apiKey.Status);
                return AuthenticateResult.Fail("API key is not active.");
            }

            if (apiKey.ExpiresAt < DateTimeOffset.UtcNow)
            {
                _logger.LogWarning("API key authentication failed: key {KeyId} expired at {ExpiresAt}", apiKey.Id, apiKey.ExpiresAt);
                return AuthenticateResult.Fail("Invalid or expired API key.");
            }

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, apiKey.UserId.ToString())
            };

            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);

            _logger.LogDebug("API key authentication succeeded: key {KeyId} for user {UserId}", apiKey.Id, apiKey.UserId);

            return AuthenticateResult.Success(ticket);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "API key authentication failed due to unexpected error");
            return AuthenticateResult.Fail("Authentication failed.");
        }
    }
}
