namespace Aethria.Application.UseCases.Roadmaps.GetPageRoadmaps;

public sealed record RoadmapPageItemResponse(
    Guid Id,
    string Name,
    string? Description,
    Guid ResourceId,
    DateTimeOffset CreatedAt);
