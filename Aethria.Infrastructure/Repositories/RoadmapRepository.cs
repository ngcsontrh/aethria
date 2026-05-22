namespace Aethria.Infrastructure.Repositories;

internal class RoadmapRepository : IRoadmapRepository
{
    private readonly AppDbContext _dbContext;

    public RoadmapRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task AddAsync(Roadmap roadmap, CancellationToken cancellationToken)
    {
        _dbContext.Roadmaps.Add(roadmap);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Roadmap roadmap, CancellationToken cancellationToken)
    {
        if (_dbContext.Entry(roadmap).State == EntityState.Detached)
        {
            _dbContext.Roadmaps.Update(roadmap);
        }
        return Task.CompletedTask;
    }

    public async Task DeleteAsync(Guid roadmapId, CancellationToken cancellationToken)
    {
        var roadmap = await _dbContext.Roadmaps.FindAsync([roadmapId], cancellationToken);
        if (roadmap != null)
        {
            _dbContext.Roadmaps.Remove(roadmap);
        }
    }

    public async Task<Roadmap?> GetByIdAsync(Guid roadmapId, CancellationToken cancellationToken)
    {
        return await _dbContext.Roadmaps
            .FirstOrDefaultAsync(r => r.Id == roadmapId, cancellationToken);
    }

    public async Task<(IReadOnlyList<Roadmap> Roadmaps, int TotalCount)> GetPageByUserIdAsync(Guid userId, int pageNumber, int pageSize, CancellationToken cancellationToken)
    {
        var query = _dbContext.Roadmaps
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.CreatedAt);

        var totalCount = await query.CountAsync(cancellationToken);
        var roadmaps = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (roadmaps, totalCount);
    }
}
