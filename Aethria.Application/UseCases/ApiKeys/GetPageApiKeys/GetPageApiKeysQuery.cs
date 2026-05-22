namespace Aethria.Application.UseCases.ApiKeys.GetPageApiKeys;

public sealed record GetPageApiKeysQuery(Guid UserId, int PageNumber, int PageSize) : IRequest<GetPageApiKeysQuery, ValueTask<Result<PagedResponse<ApiKeyPageItemResponse>>>>;
