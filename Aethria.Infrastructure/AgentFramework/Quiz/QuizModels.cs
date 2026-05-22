namespace Aethria.Infrastructure.AgentFramework.Quiz;

internal sealed class QuizGeneratorInput
{
    public Guid ResourceId { get; set; }
    public string SourceContent { get; set; } = null!;
    public string? UserPrompt { get; set; }
    public int NumberOfQuestions { get; set; }
}

internal sealed class GeneratedQuestion
{
    public int QuestionNumber { get; set; }
    public string QuestionText { get; set; } = null!;
    public List<string> Options { get; set; } = [];
    public int CorrectOptionIndex { get; set; }
    public string Explanation { get; set; } = null!;
}

internal sealed class QuizGenerationWorkflowResult
{
    public bool IsSuccess { get; set; }
    public string? ErrorMessage { get; set; }
    public List<GeneratedQuestion> Questions { get; set; } = [];
}

internal sealed class QuizGeneratorOutput
{
    public Guid ResourceId { get; set; }
    public List<QuizQuestionCandidate> Questions { get; set; } = [];
}

internal sealed class QuizQuestionCandidate
{
    public string QuestionText { get; set; } = null!;
    public List<string> Options { get; set; } = [];
    public int CorrectOptionIndex { get; set; }
    public string Explanation { get; set; } = null!;
}

internal sealed class QuizReviewEditOutput
{
    public bool IsSuccess { get; set; }
    public string? ErrorMessage { get; set; }
    public List<QuizQuestionCandidate> Questions { get; set; } = [];
}
