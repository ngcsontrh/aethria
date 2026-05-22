namespace Aethria.Application.UseCases.Resources.GetPageResources;

public sealed record GetPageResourcesQuery(Guid UserId, int PageNumber, int PageSize) : IRequest<GetPageResourcesQuery, ValueTask<Result<PagedResponse<ResourcePageItemResponse>>>>;
