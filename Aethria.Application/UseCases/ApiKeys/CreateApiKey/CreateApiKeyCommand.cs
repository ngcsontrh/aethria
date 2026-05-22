namespace Aethria.Application.UseCases.ApiKeys.CreateApiKey;

public sealed record CreateApiKeyCommand(
    Guid UserId,
    string Email,
    string Name,
    int ExpirationDays) : IRequest<CreateApiKeyCommand, ValueTask<Result<CreateApiKeyResponse>>>;
