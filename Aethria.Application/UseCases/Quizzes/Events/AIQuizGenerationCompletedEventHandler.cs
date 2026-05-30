using DispatchR.Abstractions.Notification;
using Aethria.Application.UseCases.Notifications;
using Aethria.Application.UseCases.Notifications.GetPageNotifications;

namespace Aethria.Application.UseCases.Quizzes.Events;

public sealed class AIQuizGenerationCompletedEventHandler : INotificationHandler<AIQuizGenerationCompletedEvent>
{
    private readonly IQuizRepository _quizRepository;
    private readonly INotificationRepository _notificationRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationEventPublisher _notificationEventPublisher;

    public AIQuizGenerationCompletedEventHandler(
        IQuizRepository quizRepository,
        INotificationRepository notificationRepository,
        IUnitOfWork unitOfWork,
        INotificationEventPublisher notificationEventPublisher)
    {
        _quizRepository = quizRepository;
        _notificationRepository = notificationRepository;
        _unitOfWork = unitOfWork;
        _notificationEventPublisher = notificationEventPublisher;
    }

    public async ValueTask Handle(AIQuizGenerationCompletedEvent notification, CancellationToken cancellationToken)
    {
        var quiz = await _quizRepository.GetByIdAsync(notification.QuizId, cancellationToken);
        if (quiz is null)
        {
            return;
        }

        var now = DateTimeOffset.UtcNow;
        var userNotification = new Notification
        {
            Id = Guid.NewGuid(),
            UserId = notification.UserId,
            Type = NotificationType.QuizGenerated.Value,
            Data = new Dictionary<string, string>
            {
                [NotificationDataKeys.QuizName] = quiz.Name
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
