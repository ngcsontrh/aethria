namespace Aethria.Application.UseCases.Notifications.GetPageNotifications;

public sealed class GetPageNotificationsQueryHandler : IRequestHandler<GetPageNotificationsQuery, ValueTask<Result<PagedResponse<NotificationPageItemResponse>>>>
{
    private readonly INotificationRepository _notificationRepository;

    public GetPageNotificationsQueryHandler(
        INotificationRepository notificationRepository)
    {
        _notificationRepository = notificationRepository;
    }

    public async ValueTask<Result<PagedResponse<NotificationPageItemResponse>>> Handle(
        GetPageNotificationsQuery query,
        CancellationToken cancellationToken)
    {
        var (notifications, totalCount) = await _notificationRepository.GetPageByUserIdAsync(
            query.UserId,
            query.PageNumber,
            query.PageSize,
            query.IsRead,
            cancellationToken);

        var items = notifications.Select(n => new NotificationPageItemResponse(
            Id: n.Id,
            Type: n.Type,
            Data: n.Data,
            IsRead: n.IsRead,
            CreatedAt: n.CreatedAt,
            UpdatedAt: n.UpdatedAt)).ToList();

        return Result.Ok(new PagedResponse<NotificationPageItemResponse>(
            items,
            totalCount,
            query.PageNumber,
            query.PageSize));
    }
}
