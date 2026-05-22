namespace Aethria.Domain.Repositories;

public interface IRoadmapRepository
{
    Task AddAsync(Roadmap roadmap, CancellationToken cancellationToken);
    Task UpdateAsync(Roadmap roadmap, CancellationToken cancellationToken);
    Task DeleteAsync(Guid roadmapId, CancellationToken cancellationToken);
    Task<Roadmap?> GetByIdAsync(Guid roadmapId, CancellationToken cancellationToken);
    Task<(IReadOnlyList<Roadmap> Roadmaps, int TotalCount)> GetPageByUserIdAsync(Guid userId, int pageNumber, int pageSize, CancellationToken cancellationToken);
}
