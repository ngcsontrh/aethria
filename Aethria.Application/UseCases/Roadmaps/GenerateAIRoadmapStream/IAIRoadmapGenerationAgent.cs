namespace Aethria.Application.UseCases.Roadmaps.GenerateAIRoadmapStream;

public interface IAIRoadmapGenerationAgent
{
    IAsyncEnumerable<GenerateAIRoadmapStreamResult> RunAsync(
        GenerateAIRoadmapStreamInput input,
        CancellationToken cancellationToken);
}

public sealed record GenerateAIRoadmapStreamInput
{
    public string SourceContent { get; init; } = string.Empty;
    public string? UserPrompt { get; init; }
}

public sealed record GenerateAIRoadmapStreamResult
{
    public string Status { get; init; } = string.Empty;
    public string? Message { get; init; }
    public string? RoadmapContent { get; init; }
    public string? MermaidDiagram { get; init; }
    public string? ErrorMessage { get; init; }

    public bool IsCompleted => Status == Statuses.Completed;
    public bool IsFailed => Status == Statuses.Failed;

    public static GenerateAIRoadmapStreamResult Progress(string status, string message) =>
        new() { Status = status, Message = message };

    public static GenerateAIRoadmapStreamResult Completed(string roadmapContent, string mermaidDiagram) =>
        new()
        {
            Status = Statuses.Completed,
            Message = "AI roadmap generation completed.",
            RoadmapContent = roadmapContent,
            MermaidDiagram = mermaidDiagram
        };

    public static GenerateAIRoadmapStreamResult Failed(string errorMessage) =>
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
