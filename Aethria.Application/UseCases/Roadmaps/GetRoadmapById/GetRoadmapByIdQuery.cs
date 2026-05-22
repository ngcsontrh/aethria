namespace Aethria.Application.UseCases.Roadmaps.GetRoadmapById;

public sealed record GetRoadmapByIdQuery(Guid RoadmapId, Guid UserId) : IRequest<GetRoadmapByIdQuery, ValueTask<Result<RoadmapDetailResponse>>>;
