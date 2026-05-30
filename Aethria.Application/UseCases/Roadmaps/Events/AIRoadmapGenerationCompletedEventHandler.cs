using DispatchR.Abstractions.Notification;

namespace Aethria.Application.UseCases.Roadmaps.Events;

public sealed class AIRoadmapGenerationCompletedEventHandler : INotificationHandler<AIRoadmapGenerationCompletedEvent>
{
    private readonly IRoadmapRepository _roadmapRepository;
    private readonly INotificationRepository _notificationRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AIRoadmapGenerationCompletedEventHandler(
        IRoadmapRepository roadmapRepository,
        INotificationRepository notificationRepository,
        IUnitOfWork unitOfWork)
    {
        _roadmapRepository = roadmapRepository;
        _notificationRepository = notificationRepository;
        _unitOfWork = unitOfWork;
    }

    public async ValueTask Handle(AIRoadmapGenerationCompletedEvent notification, CancellationToken cancellationToken)
    {
        var roadmap = await _roadmapRepository.GetByIdAsync(notification.RoadmapId, cancellationToken);
        if (roadmap is null)
        {
            return;
        }

        var now = DateTimeOffset.UtcNow;
        var userNotification = new Notification
        {
            Id = Guid.CreateVersion7(),
            UserId = notification.UserId,
            Type = NotificationType.RoadmapGenerated.Value,
            Data = new Dictionary<string, string>
            {
                [NotificationDataKeys.RoadmapName] = roadmap.Name
            },
            IsRead = false,
            CreatedAt = now,
            UpdatedAt = now
        };

        await _notificationRepository.AddAsync(userNotification, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
