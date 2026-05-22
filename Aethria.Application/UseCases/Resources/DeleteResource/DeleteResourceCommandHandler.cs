using Aethria.Application.Abstractions.Storage;

namespace Aethria.Application.UseCases.Resources.DeleteResource;

public class DeleteResourceCommandHandler : IRequestHandler<DeleteResourceCommand, ValueTask<Result>>
{
    private readonly IResourceRepository _resourceRepository;
    private readonly IResourceChunkRepository _resourceChunkRepository;
    private readonly IChatSessionRepository _chatSessionRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IFileStorageService _fileStorageService;

    public DeleteResourceCommandHandler(
        IResourceRepository resourceRepository,
        IResourceChunkRepository resourceChunkRepository,
        IChatSessionRepository chatSessionRepository,
        IUnitOfWork unitOfWork,
        IFileStorageService fileStorageService)
    {
        _resourceRepository = resourceRepository;
        _resourceChunkRepository = resourceChunkRepository;
        _chatSessionRepository = chatSessionRepository;
        _unitOfWork = unitOfWork;
        _fileStorageService = fileStorageService;
    }

    public async ValueTask<Result> Handle(DeleteResourceCommand request, CancellationToken cancellationToken)
    {
        var resource = await _resourceRepository.GetByIdAsync(request.ResourceId, cancellationToken);
        if (resource == null || resource.UserId != request.UserId)
        {
            return Result.Fail(new NotFoundError($"Resource with ID {request.ResourceId} not found."));
        }

        var fileUri = resource.FileUri;

        try
        {
            await _unitOfWork.BeginTransactionAsync(cancellationToken);

            await _chatSessionRepository.DeleteAllByResourceIdAsync(request.ResourceId, request.UserId, cancellationToken);
            await _resourceChunkRepository.DeleteAllByResourceIdAsync(request.ResourceId, cancellationToken);
            await _resourceRepository.DeleteAsync(request.ResourceId, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);
        }
        catch (Exception)
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }

        try
        {
            await _fileStorageService.DeleteFileAsync(fileUri, cancellationToken);
        }
        catch (Exception)
        {
        }

        return Result.Ok();
    }
}
