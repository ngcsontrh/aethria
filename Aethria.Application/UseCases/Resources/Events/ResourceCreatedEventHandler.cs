using DispatchR.Abstractions.Notification;
using Aethria.Application.UseCases.Notifications;
using Aethria.Application.UseCases.Notifications.GetPageNotifications;

namespace Aethria.Application.UseCases.Resources.Events;

public sealed class ResourceCreatedEventHandler : INotificationHandler<ResourceCreatedEvent>
{
    private readonly IResourceRepository _resourceRepository;
    private readonly INotificationRepository _notificationRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationEventPublisher _notificationEventPublisher;

    public ResourceCreatedEventHandler(
        IResourceRepository resourceRepository,
        INotificationRepository notificationRepository,
        IUnitOfWork unitOfWork,
        INotificationEventPublisher notificationEventPublisher)
    {
        _resourceRepository = resourceRepository;
        _notificationRepository = notificationRepository;
        _unitOfWork = unitOfWork;
        _notificationEventPublisher = notificationEventPublisher;
    }

    public async ValueTask Handle(ResourceCreatedEvent notification, CancellationToken cancellationToken)
    {
        var resource = await _resourceRepository.GetByIdAsync(notification.ResourceId, cancellationToken);
        if (resource is null)
        {
            return;
        }

        var now = DateTimeOffset.UtcNow;
        var userNotification = new Notification
        {
            Id = Guid.NewGuid(),
            UserId = notification.UserId,
            Type = NotificationType.ResourceCreated.Value,
            Data = new Dictionary<string, string>
            {
                [NotificationDataKeys.ResourceName] = resource.Name
            },
            IsRead = false,
            CreatedAt = now,
            UpdatedAt = now
        };

        await _notificationRepository.AddAsync(userNotification, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _notificationEventPublisher.PublishCreatedAsync(
            notification.UserId,
            new NotificationPageItemResponse(
                Id: userNotification.Id,
                Type: userNotification.Type,
                Data: userNotification.Data,
                IsRead: userNotification.IsRead,
                CreatedAt: userNotification.CreatedAt,
                UpdatedAt: userNotification.UpdatedAt),
            cancellationToken);
    }
}
