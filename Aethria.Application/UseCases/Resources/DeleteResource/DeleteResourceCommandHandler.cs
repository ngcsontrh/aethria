using Aethria.Application.Abstractions.Storage;
using Microsoft.Extensions.Logging;

namespace Aethria.Application.UseCases.Resources.DeleteResource;

public class DeleteResourceCommandHandler : IRequestHandler<DeleteResourceCommand, ValueTask<Result>>
{
    private readonly IResourceRepository _resourceRepository;
    private readonly IResourceChunkVectorStore _resourceChunkVectorStore;
    private readonly IChatSessionRepository _chatSessionRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IFileStorageService _fileStorageService;
    private readonly ILogger<DeleteResourceCommandHandler> _logger;

    public DeleteResourceCommandHandler(
        IResourceRepository resourceRepository,
        IResourceChunkVectorStore resourceChunkVectorStore,
        IChatSessionRepository chatSessionRepository,
        IUnitOfWork unitOfWork,
        IFileStorageService fileStorageService,
        ILogger<DeleteResourceCommandHandler> logger)
    {
        _resourceRepository = resourceRepository;
        _resourceChunkVectorStore = resourceChunkVectorStore;
        _chatSessionRepository = chatSessionRepository;
        _unitOfWork = unitOfWork;
        _fileStorageService = fileStorageService;
        _logger = logger;
    }

    public async ValueTask<Result> Handle(DeleteResourceCommand request, CancellationToken cancellationToken)
    {
        var resource = await _resourceRepository.GetByIdAsync(request.ResourceId, cancellationToken);
        if (resource == null || resource.UserId != request.UserId)
        {
            return Result.Fail(new NotFoundError($"Resource with ID {request.ResourceId} not found."));
        }

        var fileUri = resource.FileUri;

        await _unitOfWork.ExecuteInTransactionAsync(async ct =>
        {
            await _chatSessionRepository.DeleteAllByResourceIdAsync(request.ResourceId, request.UserId, ct);
            await _resourceChunkVectorStore.DeleteByResourceIdAsync(request.ResourceId, ct);
            await _resourceRepository.DeleteAsync(request.ResourceId, ct);
            await _unitOfWork.SaveChangesAsync(ct);
        }, cancellationToken);
        try
        {
            await _fileStorageService.DeleteFileAsync(fileUri, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete file from storage for resource {ResourceId} with URI {FileUri}", request.ResourceId, fileUri);
        }

        return Result.Ok();
    }
}
