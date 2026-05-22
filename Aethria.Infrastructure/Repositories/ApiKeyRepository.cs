using System.Security.Cryptography;
using System.Text;

namespace Aethria.Infrastructure.Repositories;

internal class ApiKeyRepository : IApiKeyRepository
{
    private readonly AppDbContext _dbContext;

    public ApiKeyRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task AddAsync(ApiKey apiKey, CancellationToken cancellationToken)
    {
        _dbContext.ApiKeys.Add(apiKey);
        return Task.CompletedTask;
    }

    public async Task<ApiKey?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _dbContext.ApiKeys
            .FirstOrDefaultAsync(k => k.Id == id, cancellationToken);
    }

    public async Task<ApiKey?> GetByTokenHashAsync(string tokenHash, CancellationToken cancellationToken)
    {
        var activeKeys = await _dbContext.ApiKeys
            .Where(k => k.Status == ApiKeyStatus.Active)
            .ToListAsync(cancellationToken);

        var targetBytes = Encoding.UTF8.GetBytes(tokenHash);

        foreach (var key in activeKeys)
        {
            var candidateBytes = Encoding.UTF8.GetBytes(key.TokenHash);

            if (candidateBytes.Length == targetBytes.Length && CryptographicOperations.FixedTimeEquals(candidateBytes, targetBytes))
            {
                return key;
            }
        }

        return null;
    }

    public async Task<IReadOnlyList<ApiKey>> ListByUserIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        return await _dbContext.ApiKeys
            .Where(k => k.UserId == userId)
            .OrderByDescending(k => k.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<(IReadOnlyList<ApiKey> ApiKeys, int TotalCount)> GetPageByUserIdAsync(
        Guid userId, int pageNumber, int pageSize, CancellationToken cancellationToken)
    {
        var query = _dbContext.ApiKeys
            .Where(k => k.UserId == userId)
            .OrderByDescending(k => k.CreatedAt);

        var totalCount = await query.CountAsync(cancellationToken);
        var apiKeys = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (apiKeys, totalCount);
    }

    public async Task<int> GetActiveCountByUserIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        return await _dbContext.ApiKeys
            .CountAsync(k => k.UserId == userId && k.Status == ApiKeyStatus.Active, cancellationToken);
    }

    public async Task<IReadOnlyList<ApiKey>> ListExpiredActiveKeysAsync(int maxCount, CancellationToken cancellationToken)
    {
        return await _dbContext.ApiKeys
            .Where(k => k.ExpiresAt < DateTimeOffset.UtcNow && k.Status == ApiKeyStatus.Active)
            .Take(maxCount)
            .ToListAsync(cancellationToken);
    }

    public Task UpdateAsync(ApiKey apiKey, CancellationToken cancellationToken)
    {
        if (_dbContext.Entry(apiKey).State == EntityState.Detached)
        {
            _dbContext.ApiKeys.Update(apiKey);
        }
        return Task.CompletedTask;
    }

    public Task UpdateRangeAsync(IEnumerable<ApiKey> apiKeys, CancellationToken cancellationToken)
    {
        foreach (var apiKey in apiKeys)
        {
            if (_dbContext.Entry(apiKey).State == EntityState.Detached)
            {
                _dbContext.ApiKeys.Update(apiKey);
            }
        }
        return Task.CompletedTask;
    }
}
