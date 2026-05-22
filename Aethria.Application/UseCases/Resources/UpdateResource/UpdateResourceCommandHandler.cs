namespace Aethria.Application.UseCases.Resources.UpdateResource;

public class UpdateResourceCommandHandler : IRequestHandler<UpdateResourceCommand, ValueTask<Result>>
{
    private readonly IResourceRepository _resourceRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateResourceCommandHandler(
        IResourceRepository resourceRepository,
        IUnitOfWork unitOfWork)
    {
        _resourceRepository = resourceRepository;
        _unitOfWork = unitOfWork;
    }

    public async ValueTask<Result> Handle(UpdateResourceCommand request, CancellationToken cancellationToken)
    {
        var trimmedName = request.Name?.Trim() ?? string.Empty;

        var resource = await _resourceRepository.GetByIdAsync(request.ResourceId, cancellationToken);
        if (resource == null || resource.UserId != request.UserId)
        {
            return Result.Fail(new NotFoundError($"Resource with ID {request.ResourceId} not found."));
        }

        resource.Name = trimmedName;
        resource.Description = request.Description;
        resource.UpdatedAt = DateTimeOffset.UtcNow;

        await _resourceRepository.UpdateAsync(resource, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }
}
