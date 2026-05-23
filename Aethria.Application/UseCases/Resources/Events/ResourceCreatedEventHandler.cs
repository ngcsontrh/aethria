using DispatchR.Abstractions.Notification;

namespace Aethria.Application.UseCases.Resources.Events;

public sealed class ResourceCreatedEventHandler : INotificationHandler<ResourceCreatedEvent>
{
    private readonly IResourceRepository _resourceRepository;
    private readonly INotificationRepository _notificationRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ResourceCreatedEventHandler(
        IResourceRepository resourceRepository,
        INotificationRepository notificationRepository,
        IUnitOfWork unitOfWork)
    {
        _resourceRepository = resourceRepository;
        _notificationRepository = notificationRepository;
        _unitOfWork = unitOfWork;
    }

    public async ValueTask Handle(ResourceCreatedEvent notification, CancellationToken cancellationToken)
    {
        var resource = await _resourceRepository.GetByIdAsync(notification.ResourceId, cancellationToken);
        var resourceName = resource?.Name ?? "Resource";

        var now = DateTimeOffset.UtcNow;
        var userNotification = new Notification
        {
            Id = Guid.NewGuid(),
            UserId = notification.UserId,
            Type = NotificationType.ResourceCreated.Value,
            Data = new Dictionary<string, string>
            {
                [NotificationDataKeys.ResourceName] = resourceName
            },
            IsRead = false,
            CreatedAt = now,
            UpdatedAt = now
        };

        await _notificationRepository.AddAsync(userNotification, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
