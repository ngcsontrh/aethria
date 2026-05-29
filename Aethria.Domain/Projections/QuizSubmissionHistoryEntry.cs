namespace Aethria.Domain.Projections;

public record QuizSubmissionHistoryEntry(
    Guid SubmissionId,
    Guid QuizVersionId,
    int VersionNumber,
    int Score,
    int TotalQuestions,
    bool IsPassed,
    DateTimeOffset SubmittedAt);
