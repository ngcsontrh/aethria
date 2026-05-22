using Aethria.Application.Abstractions.Storage;

namespace Aethria.Application.UseCases.Resources.DownloadResource;

public sealed class DownloadResourceQueryHandler : IRequestHandler<DownloadResourceQuery, ValueTask<Result<DownloadResourceResponse>>>
{
    private readonly IResourceRepository _resourceRepository;
    private readonly IFileStorageService _fileStorageService;

    public DownloadResourceQueryHandler(
        IResourceRepository resourceRepository,
        IFileStorageService fileStorageService)
    {
        _resourceRepository = resourceRepository;
        _fileStorageService = fileStorageService;
    }

    private const string DefaultContentType = "application/octet-stream";

    public async ValueTask<Result<DownloadResourceResponse>> Handle(DownloadResourceQuery query, CancellationToken cancellationToken)
    {
        var resource = await _resourceRepository.GetByIdAsync(query.ResourceId, cancellationToken);
        if (resource is null || resource.UserId != query.UserId)
        {
            return Result.Fail(new NotFoundError("Resource", query.ResourceId.ToString()));
        }

        var stream = await _fileStorageService.DownloadFileAsync(resource.FileUri, cancellationToken);
        var contentType = string.IsNullOrWhiteSpace(resource.FileType) ? DefaultContentType : resource.FileType;

        return Result.Ok(new DownloadResourceResponse(
            Content: stream,
            ContentType: contentType,
            FileName: GetDownloadFileName(resource)));
    }

    private static string GetDownloadFileName(Resource resource)
    {
        if (Uri.TryCreate(resource.FileUri, UriKind.Absolute, out var uri))
        {
            var blobFileName = Path.GetFileName(uri.LocalPath);
            if (!string.IsNullOrWhiteSpace(blobFileName))
            {
                return blobFileName;
            }
        }

        return string.IsNullOrWhiteSpace(resource.Name) ? resource.Id.ToString() : resource.Name;
    }
}
