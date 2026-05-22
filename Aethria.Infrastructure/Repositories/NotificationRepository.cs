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

    public async Task<(IReadOnlyList<Notification> Notifications, int TotalCount)> GetPageByUserIdAsync(Guid userId, int pageNumber, int pageSize, CancellationToken cancellationToken)
    {
        var query = _dbContext.Notifications
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt);

        var totalCount = await query.CountAsync(cancellationToken);
        var notifications = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (notifications, totalCount);
    }
}
