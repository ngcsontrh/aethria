namespace Aethria.Application.UseCases.Roadmaps.DeleteRoadmap;

public sealed record DeleteRoadmapCommand(Guid RoadmapId, Guid UserId) : IRequest<DeleteRoadmapCommand, ValueTask<Result>>;
