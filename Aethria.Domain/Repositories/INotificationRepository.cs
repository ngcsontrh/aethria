namespace Aethria.Domain.Repositories;

public interface INotificationRepository
{
    Task AddAsync(Notification notification, CancellationToken cancellationToken);
    Task UpdateAsync(Notification notification, CancellationToken cancellationToken);
    Task<Notification?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<(IReadOnlyList<Notification> Notifications, int TotalCount)> GetPageByUserIdAsync(Guid userId, int pageNumber, int pageSize, CancellationToken cancellationToken);
}
