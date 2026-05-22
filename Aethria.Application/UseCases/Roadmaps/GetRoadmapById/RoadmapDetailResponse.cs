namespace Aethria.Application.UseCases.Roadmaps.GetRoadmapById;

public sealed record RoadmapDetailResponse(
    Guid Id,
    string Name,
    string? Description,
    string Content,
    string? Mermaid,
    Guid ResourceId,
    DateTimeOffset CreatedAt);
