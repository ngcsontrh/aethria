namespace Aethria.Application.UseCases.Quizzes.CreateAIQuizStream;

public interface IAIQuizGenerationWorkflow
{
    IAsyncEnumerable<CreateAIQuizStreamResult> RunAsync(CreateAIQuizStreamInput input, CancellationToken cancellationToken);
}

public sealed record CreateAIQuizStreamInput
{
    public Guid ResourceId { get; init; }
    public string SourceContent { get; init; } = string.Empty;
    public string? UserPrompt { get; init; }
    public int NumberOfQuestions { get; init; }
}

public sealed record CreateAIQuizStreamResult
{
    public string Status { get; init; } = string.Empty;
    public string? Message { get; init; }
    public IReadOnlyList<AIQuizQuestion> Questions { get; init; } = [];
    public string? ErrorMessage { get; init; }

    public bool IsCompleted => Status == Statuses.Completed;
    public bool IsFailed => Status == Statuses.Failed;

    public static CreateAIQuizStreamResult Progress(string status, string message) =>
        new() { Status = status, Message = message };

    public static CreateAIQuizStreamResult Completed(IReadOnlyList<AIQuizQuestion> questions) =>
        new() { Status = Statuses.Completed, Message = "AI quiz generation completed.", Questions = questions };

    public static CreateAIQuizStreamResult Failed(string errorMessage) =>
        new() { Status = Statuses.Failed, Message = "AI quiz generation failed.", ErrorMessage = errorMessage };

    public static class Statuses
    {
        public const string Started = "Started";
        public const string GeneratingQuestions = "GeneratingQuestions";
        public const string Completed = "Completed";
        public const string Failed = "Failed";
    }
}

public sealed record AIQuizQuestion
{
    public int QuestionNumber { get; init; }
    public string QuestionText { get; init; } = string.Empty;
    public IReadOnlyList<string> Options { get; init; } = [];
    public int CorrectOptionIndex { get; init; }
    public string Explanation { get; init; } = string.Empty;
}
