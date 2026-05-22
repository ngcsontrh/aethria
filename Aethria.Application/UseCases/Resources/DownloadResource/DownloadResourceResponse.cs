namespace Aethria.Application.UseCases.Resources.DownloadResource;

public sealed record DownloadResourceResponse(
    Stream Content,
    string ContentType,
    string FileName);
