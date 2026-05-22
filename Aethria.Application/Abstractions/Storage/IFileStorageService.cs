namespace Aethria.Application.Abstractions.Storage;

public interface IFileStorageService
{
    Task<string> UploadFileAsync(
        Stream stream,
        string containerName,
        string fileName,
        string contentType,
        CancellationToken cancellationToken);

    Task<Stream> DownloadFileAsync(
        string fileUri,
        CancellationToken cancellationToken);

    Task DeleteFileAsync(
        string fileUri,
        CancellationToken cancellationToken);
}
