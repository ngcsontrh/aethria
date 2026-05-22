namespace Aethria.Domain.Repositories;

public interface IMentorRepository
{
    Task AddAsync(Mentor mentor, CancellationToken cancellationToken);
    Task UpdateAsync(Mentor mentor, CancellationToken cancellationToken);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken);
    Task<Mentor?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<(IReadOnlyList<Mentor> Mentors, int TotalCount)> GetPageByUserIdAsync(Guid userId, int pageNumber, int pageSize, CancellationToken cancellationToken);
}
