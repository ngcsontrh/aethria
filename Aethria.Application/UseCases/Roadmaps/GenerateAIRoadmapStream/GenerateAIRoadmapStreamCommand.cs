namespace Aethria.Application.UseCases.Roadmaps.GenerateAIRoadmapStream;

public sealed record GenerateAIRoadmapStreamCommand(
    string Name,
    string? Description,
    Guid ResourceId,
    string? Prompt,
    Guid UserId) : IStreamRequest<GenerateAIRoadmapStreamCommand, Result<GenerateAIRoadmapStreamEvent>>;
