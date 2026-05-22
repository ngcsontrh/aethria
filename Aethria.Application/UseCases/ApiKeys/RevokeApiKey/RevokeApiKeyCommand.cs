namespace Aethria.Application.UseCases.ApiKeys.RevokeApiKey;

public sealed record RevokeApiKeyCommand(
    Guid UserId,
    Guid KeyId) : IRequest<RevokeApiKeyCommand, ValueTask<Result>>;
