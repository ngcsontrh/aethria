namespace Aethria.Application.UseCases.Resources.GetResourceSelector;

public sealed class GetResourceSelectorQueryHandler : IRequestHandler<GetResourceSelectorQuery, ValueTask<Result<GetResourceSelectorResponse>>>
{
    private readonly IResourceRepository _resourceRepository;

    public GetResourceSelectorQueryHandler(
        IResourceRepository resourceRepository)
    {
        _resourceRepository = resourceRepository;
    }

    public async ValueTask<Result<GetResourceSelectorResponse>> Handle(
        GetResourceSelectorQuery query,
        CancellationToken cancellationToken)
    {
        var resources = await _resourceRepository.ListBasicByUserIdAsync(
            query.UserId,
            cancellationToken);

        var items = resources
            .Select(r => new ResourceSelectorResponse(r.Id, r.Name))
            .ToList();

        return Result.Ok(new GetResourceSelectorResponse(items));
    }
}
