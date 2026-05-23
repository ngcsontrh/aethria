namespace Aethria.Infrastructure.Repositories;

internal class NotificationRepository : INotificationRepository
{
    private readonly AppDbContext _dbContext;

    public NotificationRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task AddAsync(Notification notification, CancellationToken cancellationToken)
    {
        _dbContext.Notifications.Add(notification);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Notification notification, CancellationToken cancellationToken)
    {
        if (_dbContext.Entry(notification).State == EntityState.Detached)
        {
            _dbContext.Notifications.Update(notification);
        }
        return Task.CompletedTask;
    }

    public async Task<Notification?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _dbContext.Notifications.FindAsync([id], cancellationToken);
    }

    public async Task<(IReadOnlyList<Notification> Notifications, int TotalCount)> GetPageByUserIdAsync(Guid userId, int pageNumber, int pageSize, bool? isRead, CancellationToken cancellationToken)
    {
        var query = _dbContext.Notifications
            .Where(n => n.UserId == userId);

        if (isRead.HasValue)
        {
            query = query.Where(n => n.IsRead == isRead.Value);
        }

        query = query.OrderByDescending(n => n.CreatedAt);

        var totalCount = await query.CountAsync(cancellationToken);
        var notifications = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (notifications, totalCount);
    }

    public async Task MarkAsReadAsync(Guid userId, IReadOnlyCollection<Guid> notificationIds, CancellationToken cancellationToken)
    {
        if (notificationIds.Count == 0)
        {
            return;
        }

        var ids = notificationIds.Distinct().ToArray();
        var now = DateTimeOffset.UtcNow;

        await _dbContext.Notifications
            .Where(n => n.UserId == userId && ids.Contains(n.Id) && !n.IsRead)
            .ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(n => n.IsRead, true)
                    .SetProperty(n => n.UpdatedAt, now),
                cancellationToken);
    }
}
