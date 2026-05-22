namespace Aethria.Domain.Repositories;

public interface IResourceChunkRepository
{
    Task AddRangeAsync(IEnumerable<ResourceChunk> chunks, CancellationToken cancellationToken);
    Task DeleteAllByResourceIdAsync(Guid resourceId, CancellationToken cancellationToken);
}
