using Pgvector;
using Pgvector.EntityFrameworkCore;

namespace Aethria.Infrastructure.Repositories;

internal class ResourceRepository : IResourceRepository
{
    private readonly AppDbContext _dbContext;

    public ResourceRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task AddAsync(Resource resource, CancellationToken cancellationToken)
    {
        _dbContext.Resources.Add(resource);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Resource resource, CancellationToken cancellationToken)
    {
        _dbContext.Resources.Update(resource);
        return Task.CompletedTask;
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var resource = await _dbContext.Resources.FindAsync([id], cancellationToken);
        if (resource != null)
        {
            _dbContext.Resources.Remove(resource);
        }
    }

    public async Task<Resource?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _dbContext.Resources
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    public async Task<(IReadOnlyList<Resource> Resources, int TotalCount)> GetPageByUserIdAsync(Guid userId, int pageNumber, int pageSize, CancellationToken cancellationToken)
    {
        var query = _dbContext.Resources
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.CreatedAt);

        var totalCount = await query.CountAsync(cancellationToken);
        var resources = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (resources, totalCount);
    }

    public async Task<IReadOnlyList<Resource>> ListBasicByUserIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        return await _dbContext.Resources
            .AsNoTracking()
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new Resource
            {
                Id = r.Id,
                Name = r.Name
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsByIdAndUserAsync(Guid id, Guid userId, CancellationToken cancellationToken)
    {
        return await _dbContext.Resources
            .AnyAsync(r => r.Id == id && r.UserId == userId, cancellationToken);
    }

    public async Task<bool> HasChunksAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _dbContext.ResourceChunks
            .AnyAsync(c => c.ResourceId == id, cancellationToken);
    }

    public async Task<IReadOnlyList<ResourceChunk>> ListChunksByResourceIdAsync(Guid resourceId, CancellationToken cancellationToken)
    {
        return await _dbContext.ResourceChunks
            .Where(c => c.ResourceId == resourceId)
            .OrderBy(c => c.ChunkIndex)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ResourceChunk>> ListRelevantChunksByResourceIdAsync(Guid resourceId, ReadOnlyMemory<float> embeddings, int topK, CancellationToken cancellationToken)
    {
        var vector = new Vector(embeddings);

        return await _dbContext.ResourceChunks
            .Where(c => c.ResourceId == resourceId && c.Embedding != null)
            .OrderBy(c => c.Embedding!.L2Distance(vector))
            .Take(topK)
            .ToListAsync(cancellationToken);
    }
}
