namespace Aethria.Application.Abstractions.VectorSearch;

public interface IResourceChunkVectorStore
{
    Task UpsertAsync(
        Guid resourceId,
        IReadOnlyList<ResourceChunkVectorInput> chunks,
        CancellationToken cancellationToken);

    Task DeleteByResourceIdAsync(Guid resourceId, CancellationToken cancellationToken);

    Task<bool> ExistsByResourceIdAsync(Guid resourceId, CancellationToken cancellationToken);

    Task<IReadOnlyList<ResourceChunkSearchResult>> GetRelevantChunksAsync(
        Guid resourceId,
        string query,
        int topK,
        CancellationToken cancellationToken);
}

public sealed record ResourceChunkVectorInput(
    Guid Id,
    Guid ResourceId,
    int ChunkIndex,
    string Content,
    ReadOnlyMemory<float> Embedding);

public sealed record ResourceChunkSearchResult(
    Guid Id,
    int ChunkIndex,
    string Content,
    double VectorScore);
