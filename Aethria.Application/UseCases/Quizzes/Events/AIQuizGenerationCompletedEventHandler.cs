using DispatchR.Abstractions.Notification;

namespace Aethria.Application.UseCases.Quizzes.Events;

public sealed class AIQuizGenerationCompletedEventHandler : INotificationHandler<AIQuizGenerationCompletedEvent>
{
    private readonly IQuizRepository _quizRepository;
    private readonly INotificationRepository _notificationRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AIQuizGenerationCompletedEventHandler(
        IQuizRepository quizRepository,
        INotificationRepository notificationRepository,
        IUnitOfWork unitOfWork)
    {
        _quizRepository = quizRepository;
        _notificationRepository = notificationRepository;
        _unitOfWork = unitOfWork;
    }

    public async ValueTask Handle(AIQuizGenerationCompletedEvent notification, CancellationToken cancellationToken)
    {
        var quiz = await _quizRepository.GetByIdAsync(notification.QuizId, cancellationToken);
        var quizName = quiz?.Name ?? "AI Quiz";

        var now = DateTimeOffset.UtcNow;
        var userNotification = new Notification
        {
            Id = Guid.NewGuid(),
            UserId = notification.UserId,
            Name = "Quiz Generated",
            Message = $"Your quiz '{quizName}' has been successfully generated.",
            IsRead = false,
            CreatedAt = now,
            UpdatedAt = now
        };

        await _notificationRepository.AddAsync(userNotification, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
