namespace Aethria.Application.UseCases.Notifications.MarkNotificationAsRead;

public sealed class MarkNotificationAsReadCommandHandler : IRequestHandler<MarkNotificationAsReadCommand, ValueTask<Result>>
{
    private readonly INotificationRepository _notificationRepository;
    private readonly IUnitOfWork _unitOfWork;

    public MarkNotificationAsReadCommandHandler(
        INotificationRepository notificationRepository,
        IUnitOfWork unitOfWork)
    {
        _notificationRepository = notificationRepository;
        _unitOfWork = unitOfWork;
    }

    public async ValueTask<Result> Handle(
        MarkNotificationAsReadCommand command,
        CancellationToken cancellationToken)
    {
        var notification = await _notificationRepository.GetByIdAsync(command.NotificationId, cancellationToken);

        if (notification is null || notification.UserId != command.UserId)
        {
            return Result.Fail(new NotFoundError("Notification", command.NotificationId.ToString()));
        }

        if (notification.IsRead)
        {
            return Result.Ok();
        }

        notification.IsRead = true;
        notification.UpdatedAt = DateTimeOffset.UtcNow;

        await _notificationRepository.UpdateAsync(notification, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }
}

