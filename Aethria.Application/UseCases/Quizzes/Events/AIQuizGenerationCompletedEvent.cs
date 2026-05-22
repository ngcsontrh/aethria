using DispatchR.Abstractions.Notification;

namespace Aethria.Application.UseCases.Quizzes.Events;

public sealed record AIQuizGenerationCompletedEvent(
    Guid QuizId,
    Guid UserId) : INotification;
