namespace Aethria.Application.UseCases.Roadmaps.GenerateAIRoadmap;

public interface IAIRoadmapGenerationAgent
{
    IAsyncEnumerable<GenerateAIRoadmapResult> RunAsync(
        GenerateAIRoadmapInput input,
        CancellationToken cancellationToken);
}

public sealed record GenerateAIRoadmapInput
{
    public string SourceContent { get; init; } = string.Empty;
    public string? UserPrompt { get; init; }
}

public sealed record GenerateAIRoadmapResult
{
    public string Status { get; init; } = string.Empty;
    public string? Message { get; init; }
    public string? RoadmapContent { get; init; }
    public string? MermaidDiagram { get; init; }
    public string? ErrorMessage { get; init; }

    public bool IsCompleted => Status == Statuses.Completed;
    public bool IsFailed => Status == Statuses.Failed;

    public static GenerateAIRoadmapResult Progress(string status, string message) =>
        new() { Status = status, Message = message };

    public static GenerateAIRoadmapResult Completed(string roadmapContent, string mermaidDiagram) =>
        new()
        {
            Status = Statuses.Completed,
            Message = "AI roadmap generation completed.",
            RoadmapContent = roadmapContent,
            MermaidDiagram = mermaidDiagram
        };

    public static GenerateAIRoadmapResult Failed(string errorMessage) =>
        new()
        {
            Status = Statuses.Failed,
            Message = "AI roadmap generation failed.",
            ErrorMessage = errorMessage
        };

    public static class Statuses
    {
        public const string Started = "Started";
        public const string GeneratingRoadmap = "GeneratingRoadmap";
        public const string Completed = "Completed";
        public const string Failed = "Failed";
    }
}
