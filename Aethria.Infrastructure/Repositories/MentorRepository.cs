namespace Aethria.Infrastructure.Repositories;

internal class MentorRepository : IMentorRepository
{
    private readonly AppDbContext _dbContext;

    public MentorRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task AddAsync(Mentor mentor, CancellationToken cancellationToken)
    {
        _dbContext.Mentors.Add(mentor);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Mentor mentor, CancellationToken cancellationToken)
    {
        if (_dbContext.Entry(mentor).State == EntityState.Detached)
        {
            _dbContext.Mentors.Update(mentor);
        }
        return Task.CompletedTask;
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var mentor = await _dbContext.Mentors.FindAsync([id], cancellationToken);
        if (mentor != null)
        {
            _dbContext.Mentors.Remove(mentor);
        }
    }

    public async Task<Mentor?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _dbContext.Mentors
            .FirstOrDefaultAsync(m => m.Id == id, cancellationToken);
    }

    public async Task<(IReadOnlyList<Mentor> Mentors, int TotalCount)> GetPageByUserIdAsync(Guid userId, int pageNumber, int pageSize, CancellationToken cancellationToken)
    {
        var query = _dbContext.Mentors
            .Where(m => m.UserId == userId)
            .OrderByDescending(m => m.CreatedAt);

        var totalCount = await query.CountAsync(cancellationToken);
        var mentors = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (mentors, totalCount);
    }
}
