namespace Aethria.Application.UseCases.ApiKeys.CreateApiKey;

public sealed class CreateApiKeyCommandHandler : IRequestHandler<CreateApiKeyCommand, ValueTask<Result<CreateApiKeyResponse>>>
{
    private readonly IApiKeyRepository _apiKeyRepository;
    private readonly IApiKeyTokenService _apiKeyTokenService;
    private readonly IUnitOfWork _unitOfWork;

    public CreateApiKeyCommandHandler(
        IApiKeyRepository apiKeyRepository,
        IApiKeyTokenService apiKeyTokenService,
        IUnitOfWork unitOfWork)
    {
        _apiKeyRepository = apiKeyRepository;
        _apiKeyTokenService = apiKeyTokenService;
        _unitOfWork = unitOfWork;
    }

    private const int MaxActiveKeys = 10;

    public async ValueTask<Result<CreateApiKeyResponse>> Handle(CreateApiKeyCommand command, CancellationToken cancellationToken)
    {
        var activeKeyCount = await _apiKeyRepository.GetActiveCountByUserIdAsync(command.UserId, cancellationToken);
        if (activeKeyCount >= MaxActiveKeys)
        {
            return Result.Fail(new ConflictError("The maximum number of active API keys (10) has been reached."));
        }

        var now = DateTimeOffset.UtcNow;
        var expiresAt = now.AddDays(command.ExpirationDays);
        var keyId = Guid.CreateVersion7();

        var tokenResult = _apiKeyTokenService.GenerateToken(command.UserId, command.Email, keyId, expiresAt);

        var apiKey = new ApiKey
        {
            Id = keyId,
            UserId = command.UserId,
            Name = command.Name,
            TokenHash = tokenResult.TokenHash,
            ExpiresAt = expiresAt,
            Status = ApiKeyStatus.Active,
            LastFourChars = tokenResult.LastFourChars,
            CreatedAt = now,
            UpdatedAt = now
        };

        await _apiKeyRepository.AddAsync(apiKey, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var response = new CreateApiKeyResponse(
            apiKey.Id,
            apiKey.Name,
            tokenResult.Token,
            apiKey.ExpiresAt,
            apiKey.CreatedAt);

        return Result.Ok(response);
    }
}
