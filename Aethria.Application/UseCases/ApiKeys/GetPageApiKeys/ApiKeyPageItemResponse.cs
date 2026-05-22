namespace Aethria.Application.UseCases.ApiKeys.GetPageApiKeys;

public sealed record ApiKeyPageItemResponse(
    Guid Id,
    string Name,
    string LastFourChars,
    string Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset ExpiresAt);
