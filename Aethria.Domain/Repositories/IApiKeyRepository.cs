namespace Aethria.Domain.Repositories;

public interface IApiKeyRepository
{
    Task AddAsync(ApiKey apiKey, CancellationToken cancellationToken);
    Task<ApiKey?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<ApiKey?> GetByTokenHashAsync(string tokenHash, CancellationToken cancellationToken);
    Task<IReadOnlyList<ApiKey>> ListByUserIdAsync(Guid userId, CancellationToken cancellationToken);
    Task<(IReadOnlyList<ApiKey> ApiKeys, int TotalCount)> GetPageByUserIdAsync(Guid userId, int pageNumber, int pageSize, CancellationToken cancellationToken);
    Task<int> GetActiveCountByUserIdAsync(Guid userId, CancellationToken cancellationToken);
    Task<IReadOnlyList<ApiKey>> ListExpiredActiveKeysAsync(int maxCount, CancellationToken cancellationToken);
    Task UpdateAsync(ApiKey apiKey, CancellationToken cancellationToken);
    Task UpdateRangeAsync(IEnumerable<ApiKey> apiKeys, CancellationToken cancellationToken);
}
