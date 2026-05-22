namespace Aethria.Application.UseCases.Roadmaps.GetPageRoadmaps;

public sealed record GetPageRoadmapsQuery(Guid UserId, int PageNumber, int PageSize) : IRequest<GetPageRoadmapsQuery, ValueTask<Result<PagedResponse<RoadmapPageItemResponse>>>>;
