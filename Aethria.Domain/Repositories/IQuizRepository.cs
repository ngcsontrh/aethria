namespace Aethria.Domain.Repositories;

public interface IQuizRepository
{
    Task AddAsync(Quiz quiz, CancellationToken cancellationToken);
    Task<Quiz?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<(IReadOnlyList<Quiz> Quizzes, int TotalCount)> GetPageByUserIdAsync(
        Guid userId, int pageNumber, int pageSize, CancellationToken cancellationToken);
    Task<Quiz?> GetByIdWithQuestionsAsync(Guid id, CancellationToken cancellationToken);

    Task<Quiz?> GetByIdWithCurrentVersionAsync(Guid id, CancellationToken cancellationToken);

    Task<Quiz?> GetByIdWithQuestionsAndVersionsAsync(Guid id, CancellationToken cancellationToken);

    Task DeleteAsync(Quiz quiz, CancellationToken cancellationToken);
    Task DeleteSubmissionsByQuizIdAsync(Guid quizId, CancellationToken cancellationToken);

    Task AddSubmissionAsync(QuizSubmission submission, CancellationToken cancellationToken);

    Task<QuizSubmission?> GetSubmissionByIdAsync(Guid submissionId, CancellationToken cancellationToken);

    Task<(IReadOnlyList<QuizSubmissionHistoryEntry> Submissions, int TotalCount)> GetSubmissionHistoryByQuizIdAsync(
        Guid quizId,
        Guid userId,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken);

    Task<QuizVersion?> GetVersionByIdWithSnapshotsAsync(Guid quizVersionId, CancellationToken cancellationToken);
}

public record QuizSubmissionHistoryEntry(
    Guid SubmissionId,
    Guid QuizVersionId,
    int VersionNumber,
    int Score,
    int TotalQuestions,
    bool IsPassed,
    DateTimeOffset SubmittedAt);
