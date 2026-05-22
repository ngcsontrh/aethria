namespace Aethria.Application.UseCases.Resources.GetResourceById;

public class GetResourceByIdQueryHandler : IRequestHandler<GetResourceByIdQuery, ValueTask<Result<GetResourceByIdResponse>>>
{
    private readonly IResourceRepository _resourceRepository;

    public GetResourceByIdQueryHandler(
        IResourceRepository resourceRepository)
    {
        _resourceRepository = resourceRepository;
    }

    public async ValueTask<Result<GetResourceByIdResponse>> Handle(GetResourceByIdQuery request, CancellationToken cancellationToken)
    {
        var resource = await _resourceRepository.GetByIdAsync(request.ResourceId, cancellationToken);
        if (resource == null || resource.UserId != request.UserId)
        {
            return Result.Fail(new NotFoundError($"Resource with ID {request.ResourceId} not found."));
        }

        var fileName = Path.GetFileName(new Uri(resource.FileUri).LocalPath);
        var downloadUrl = $"/api/resources/{resource.Id}/download";

        return Result.Ok(new GetResourceByIdResponse(
            resource.Id,
            resource.Name,
            resource.Description,
            fileName,
            downloadUrl,
            resource.CreatedAt,
            resource.UpdatedAt
        ));
    }
}
