namespace Aethria.Application.UseCases.ApiKeys.GetPageApiKeys;

public sealed class GetPageApiKeysQueryHandler : IRequestHandler<GetPageApiKeysQuery, ValueTask<Result<PagedResponse<ApiKeyPageItemResponse>>>>
{
    private readonly IApiKeyRepository _apiKeyRepository;

    public GetPageApiKeysQueryHandler(
        IApiKeyRepository apiKeyRepository)
    {
        _apiKeyRepository = apiKeyRepository;
    }

    public async ValueTask<Result<PagedResponse<ApiKeyPageItemResponse>>> Handle(
        GetPageApiKeysQuery query, CancellationToken cancellationToken)
    {
        var (apiKeys, totalCount) = await _apiKeyRepository.GetPageByUserIdAsync(
            query.UserId, query.PageNumber, query.PageSize, cancellationToken);

        var items = apiKeys.Select(k => new ApiKeyPageItemResponse(
            Id: k.Id,
            Name: k.Name,
            LastFourChars: k.LastFourChars,
            Status: k.Status.Value,
            CreatedAt: k.CreatedAt,
            ExpiresAt: k.ExpiresAt)).ToList();

        return Result.Ok(new PagedResponse<ApiKeyPageItemResponse>(items, totalCount, query.PageNumber, query.PageSize));
    }
}
