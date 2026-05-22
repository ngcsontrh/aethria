namespace Aethria.Domain.Repositories;

public interface IResourceRepository
{
    Task AddAsync(Resource resource, CancellationToken cancellationToken);
    Task UpdateAsync(Resource resource, CancellationToken cancellationToken);
    Task<Resource?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<(IReadOnlyList<Resource> Resources, int TotalCount)> GetPageByUserIdAsync(Guid userId, int pageNumber, int pageSize, CancellationToken cancellationToken);
    Task<IReadOnlyList<Resource>> ListBasicByUserIdAsync(Guid userId, CancellationToken cancellationToken);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken);
    Task<bool> ExistsByIdAndUserAsync(Guid id, Guid userId, CancellationToken cancellationToken);
    Task<bool> HasChunksAsync(Guid id, CancellationToken cancellationToken);
    Task<IReadOnlyList<ResourceChunk>> ListChunksByResourceIdAsync(Guid resourceId, CancellationToken cancellationToken);
    Task<IReadOnlyList<ResourceChunk>> ListRelevantChunksByResourceIdAsync(Guid resourceId, ReadOnlyMemory<float> embeddings, int topK, CancellationToken cancellationToken);
}
