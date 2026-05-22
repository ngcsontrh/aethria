namespace Aethria.Application.UseCases.Resources.GetPageResources;

public class GetPageResourcesQueryHandler : IRequestHandler<GetPageResourcesQuery, ValueTask<Result<PagedResponse<ResourcePageItemResponse>>>>
{
    private readonly IResourceRepository _resourceRepository;

    public GetPageResourcesQueryHandler(
        IResourceRepository resourceRepository)
    {
        _resourceRepository = resourceRepository;
    }

    public async ValueTask<Result<PagedResponse<ResourcePageItemResponse>>> Handle(GetPageResourcesQuery request, CancellationToken cancellationToken)
    {
        var (resources, totalCount) = await _resourceRepository.GetPageByUserIdAsync(
            request.UserId,
            request.PageNumber,
            request.PageSize,
            cancellationToken);

        var items = resources.Select(r => new ResourcePageItemResponse(
            r.Id,
            r.Name,
            r.Description,
            r.FileType,
            r.FileSize,
            r.CreatedAt,
            r.UpdatedAt
        )).ToList();

        return Result.Ok(new PagedResponse<ResourcePageItemResponse>(items, totalCount, request.PageNumber, request.PageSize));
    }
}
