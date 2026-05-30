namespace Aethria.Infrastructure.Repositories;

internal class QuizRepository : IQuizRepository
{
    private readonly AppDbContext _dbContext;

    public QuizRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task AddAsync(Quiz quiz, CancellationToken cancellationToken)
    {
        _dbContext.Quizzes.Add(quiz);
        return Task.CompletedTask;
    }

    public async Task<Quiz?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _dbContext.Quizzes
            .FirstOrDefaultAsync(q => q.Id == id, cancellationToken);
    }

    public async Task<(IReadOnlyList<Quiz> Quizzes, int TotalCount)> GetPageByUserIdAsync(
        Guid userId, int pageNumber, int pageSize, CancellationToken cancellationToken)
    {
        var query = _dbContext.Quizzes
            .Where(q => q.UserId == userId)
            .OrderByDescending(q => q.CreatedAt);

        var totalCount = await query.CountAsync(cancellationToken);
        var quizzes = await query
            .Include(q => q.Questions)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (quizzes, totalCount);
    }

    public async Task<Quiz?> GetByIdWithQuestionsAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _dbContext.Quizzes
            .AsSplitQuery()
            .Include(q => q.Questions)
                .ThenInclude(q => q.Options)
            .FirstOrDefaultAsync(q => q.Id == id, cancellationToken);
    }

    public async Task<Quiz?> GetByIdWithCurrentVersionAsync(Guid id, CancellationToken cancellationToken)
    {
        var currentVersionNumber = await _dbContext.Quizzes
            .Where(q => q.Id == id)
            .Select(q => (int?)q.CurrentVersionNumber)
            .FirstOrDefaultAsync(cancellationToken);

        if (currentVersionNumber is null)
        {
            return null;
        }

        return await _dbContext.Quizzes
            .AsSplitQuery()
            .Include(q => q.Versions.Where(v => v.VersionNumber == currentVersionNumber.Value))
                .ThenInclude(v => v.QuestionSnapshots)
                    .ThenInclude(qs => qs.Options)
            .FirstOrDefaultAsync(q => q.Id == id, cancellationToken);
    }

    public async Task<Quiz?> GetByIdWithQuestionsAndVersionsAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _dbContext.Quizzes
            .AsSplitQuery()
            .Include(q => q.Questions)
                .ThenInclude(q => q.Options)
            .Include(q => q.Versions)
            .FirstOrDefaultAsync(q => q.Id == id, cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var quiz = await _dbContext.Quizzes.FindAsync([id], cancellationToken);
        if (quiz != null)
        {
            _dbContext.Quizzes.Remove(quiz);
        }
    }

    public async Task DeleteQuestionsByQuizIdAsync(Guid quizId, CancellationToken cancellationToken)
    {
        await _dbContext.QuizQuestions
            .Where(q => q.QuizId == quizId)
            .ExecuteDeleteAsync(cancellationToken);
    }

    public async Task DeleteSubmissionsByQuizIdAsync(Guid quizId, CancellationToken cancellationToken)
    {
        var submissions = await _dbContext.QuizSubmissions
            .Where(s => s.QuizId == quizId)
            .ToListAsync(cancellationToken);

        _dbContext.QuizSubmissions.RemoveRange(submissions);
    }

    public Task AddSubmissionAsync(QuizSubmission submission, CancellationToken cancellationToken)
    {
        _dbContext.QuizSubmissions.Add(submission);
        return Task.CompletedTask;
    }

    public async Task<QuizSubmission?> GetSubmissionByIdAsync(Guid submissionId, CancellationToken cancellationToken)
    {
        return await _dbContext.QuizSubmissions
            .Include(s => s.Answers)
            .FirstOrDefaultAsync(s => s.Id == submissionId, cancellationToken);
    }

    public async Task<(IReadOnlyList<QuizSubmissionHistoryEntry> Submissions, int TotalCount)> GetSubmissionHistoryByQuizIdAsync(
        Guid quizId,
        Guid userId,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var query = _dbContext.QuizSubmissions
            .AsNoTracking()
            .Where(s => s.QuizId == quizId && s.UserId == userId)
            .Join(
                _dbContext.QuizVersions.AsNoTracking().Where(v => v.QuizId == quizId),
                submission => submission.QuizVersionId,
                version => version.Id,
                (submission, version) => new { Submission = submission, Version = version });

        var totalCount = await query.CountAsync(cancellationToken);
        var submissions = await query
            .OrderByDescending(x => x.Submission.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new QuizSubmissionHistoryEntry(
                x.Submission.Id,
                x.Submission.QuizVersionId,
                x.Version.VersionNumber,
                x.Submission.Score,
                x.Submission.TotalQuestions,
                x.Submission.IsPassed,
                x.Submission.CreatedAt))
            .ToListAsync(cancellationToken);

        return (submissions, totalCount);
    }

    public async Task<QuizVersion?> GetVersionByIdWithSnapshotsAsync(Guid quizVersionId, CancellationToken cancellationToken)
    {
        return await _dbContext.QuizVersions
            .AsSplitQuery()
            .Include(v => v.QuestionSnapshots)
                .ThenInclude(qs => qs.Options)
            .FirstOrDefaultAsync(v => v.Id == quizVersionId, cancellationToken);
    }
}
