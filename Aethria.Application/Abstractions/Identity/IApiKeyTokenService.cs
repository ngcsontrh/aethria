namespace Aethria.Application.Abstractions.Identity;

public interface IApiKeyTokenService
{
    ApiKeyTokenGenerationResult GenerateToken(Guid userId, string email, Guid keyId, DateTimeOffset expiresAt);
}

public sealed record ApiKeyTokenGenerationResult(
    string Token,
    string TokenHash,
    string LastFourChars);
