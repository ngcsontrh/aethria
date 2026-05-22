namespace Aethria.Infrastructure.Repositories;

internal class ResourceChunkRepository : IResourceChunkRepository
{
    private readonly AppDbContext _dbContext;

    public ResourceChunkRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddRangeAsync(IEnumerable<ResourceChunk> chunks, CancellationToken cancellationToken)
    {
        await _dbContext.ResourceChunks.AddRangeAsync(chunks, cancellationToken);
    }

    public async Task DeleteAllByResourceIdAsync(Guid resourceId, CancellationToken cancellationToken)
    {
        await _dbContext.ResourceChunks
            .Where(c => c.ResourceId == resourceId)
            .ExecuteDeleteAsync(cancellationToken);
    }
}
