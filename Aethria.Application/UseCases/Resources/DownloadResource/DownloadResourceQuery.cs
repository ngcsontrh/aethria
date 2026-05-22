namespace Aethria.Application.UseCases.Resources.DownloadResource;

public sealed record DownloadResourceQuery(Guid ResourceId, Guid UserId) : IRequest<DownloadResourceQuery, ValueTask<Result<DownloadResourceResponse>>>;
