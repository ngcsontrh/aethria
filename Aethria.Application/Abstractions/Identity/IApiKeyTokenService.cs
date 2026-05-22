namespace Aethria.Application.Abstractions.Identity;

public interface IApiKeyTokenService
{
    ApiKeyTokenGenerationResult GenerateToken(Guid userId, string email, Guid keyId, DateTimeOffset expiresAt);
}
