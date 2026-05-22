namespace Aethria.Application.UseCases.ApiKeys.RevokeApiKey;

public sealed class RevokeApiKeyCommandHandler : IRequestHandler<RevokeApiKeyCommand, ValueTask<Result>>
{
    private readonly IApiKeyRepository _apiKeyRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RevokeApiKeyCommandHandler(
        IApiKeyRepository apiKeyRepository,
        IUnitOfWork unitOfWork)
    {
        _apiKeyRepository = apiKeyRepository;
        _unitOfWork = unitOfWork;
    }

    public async ValueTask<Result> Handle(RevokeApiKeyCommand command, CancellationToken cancellationToken)
    {
        var apiKey = await _apiKeyRepository.GetByIdAsync(command.KeyId, cancellationToken);

        if (apiKey is null || apiKey.UserId != command.UserId)
        {
            return Result.Fail(new NotFoundError("ApiKey", command.KeyId.ToString()));
        }

        if (apiKey.Status == ApiKeyStatus.Revoked)
        {
            return Result.Ok();
        }

        apiKey.Status = ApiKeyStatus.Revoked;
        apiKey.RevokedAt = DateTimeOffset.UtcNow;

        await _apiKeyRepository.UpdateAsync(apiKey, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }
}
