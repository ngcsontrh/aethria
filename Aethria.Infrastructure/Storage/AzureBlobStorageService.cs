using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace Aethria.Infrastructure.Storage;

internal sealed class AzureBlobStorageService : IFileStorageService
{
    private readonly BlobServiceClient _blobServiceClient;

    public AzureBlobStorageService(BlobServiceClient blobServiceClient)
    {
        _blobServiceClient = blobServiceClient;
    }

    public async Task<string> UploadFileAsync(
        Stream stream,
        string containerName,
        string fileName,
        string contentType,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(containerName);
        ArgumentException.ThrowIfNullOrWhiteSpace(fileName);
        ArgumentException.ThrowIfNullOrWhiteSpace(contentType);

        var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);

        await containerClient.CreateIfNotExistsAsync(PublicAccessType.None, cancellationToken: cancellationToken);

        var blobClient = containerClient.GetBlobClient(fileName);

        var uploadOptions = new BlobUploadOptions
        {
            HttpHeaders = new BlobHttpHeaders
            {
                ContentType = contentType
            }
        };

        if (stream.CanSeek)
        {
            stream.Position = 0;
        }

        await blobClient.UploadAsync(
            stream,
            uploadOptions,
            cancellationToken);

        return blobClient.Uri.ToString();
    }

    public async Task<Stream> DownloadFileAsync(string fileUri, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fileUri);

        var uriBuilder = new BlobUriBuilder(new Uri(fileUri));
        var containerClient = _blobServiceClient.GetBlobContainerClient(uriBuilder.BlobContainerName);
        var blobClient = containerClient.GetBlobClient(uriBuilder.BlobName);

        var response = await blobClient.DownloadStreamingAsync(cancellationToken: cancellationToken);
        return response.Value.Content;
    }

    public async Task DeleteFileAsync(string fileUri, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fileUri);

        var uriBuilder = new BlobUriBuilder(new Uri(fileUri));
        var containerClient = _blobServiceClient.GetBlobContainerClient(uriBuilder.BlobContainerName);
        var blobClient = containerClient.GetBlobClient(uriBuilder.BlobName);

        await blobClient.DeleteIfExistsAsync(cancellationToken: cancellationToken);
    }
}
