namespace Aethria.Application.UseCases.ApiKeys.CreateApiKey;

public sealed record CreateApiKeyResponse(
    Guid Id,
    string Name,
    string Token,
    DateTimeOffset ExpiresAt,
    DateTimeOffset CreatedAt);
