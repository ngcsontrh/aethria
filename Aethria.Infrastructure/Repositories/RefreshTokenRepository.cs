namespace Aethria.Infrastructure.Repositories;

internal class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly AppDbContext _dbContext;

    public RefreshTokenRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task AddAsync(RefreshToken refreshToken, CancellationToken cancellationToken)
    {
        _dbContext.RefreshTokens.Add(refreshToken);
        return Task.CompletedTask;
    }

    public async Task<RefreshToken?> GetActiveByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _dbContext.RefreshTokens
            .FirstOrDefaultAsync(
                token => token.Id == id
                    && token.Status == RefreshTokenStatus.Active,
                cancellationToken);
    }

    public Task UpdateAsync(RefreshToken refreshToken, CancellationToken cancellationToken)
    {
        if (_dbContext.Entry(refreshToken).State == EntityState.Detached)
        {
            _dbContext.RefreshTokens.Update(refreshToken);
        }

        return Task.CompletedTask;
    }
}
