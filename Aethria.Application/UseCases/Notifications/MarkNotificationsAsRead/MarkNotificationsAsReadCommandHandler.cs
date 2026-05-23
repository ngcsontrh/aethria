namespace Aethria.Application.UseCases.Notifications.MarkNotificationsAsRead;

public sealed class MarkNotificationsAsReadCommandHandler : IRequestHandler<MarkNotificationsAsReadCommand, ValueTask<Result>>
{
    private readonly INotificationRepository _notificationRepository;

    public MarkNotificationsAsReadCommandHandler(
        INotificationRepository notificationRepository)
    {
        _notificationRepository = notificationRepository;
    }

    public async ValueTask<Result> Handle(
        MarkNotificationsAsReadCommand command,
        CancellationToken cancellationToken)
    {
        await _notificationRepository.MarkAsReadAsync(
            command.UserId,
            command.NotificationIds,
            cancellationToken);

        return Result.Ok();
    }
}
