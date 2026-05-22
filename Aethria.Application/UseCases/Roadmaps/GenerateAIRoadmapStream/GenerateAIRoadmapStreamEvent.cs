namespace Aethria.Application.UseCases.Roadmaps.GenerateAIRoadmapStream;

public sealed record GenerateAIRoadmapStreamEvent(
    string Status,
    string Message,
    Guid? RoadmapId = null);
