namespace Aethria.Application.UseCases.Roadmaps.GetPageRoadmaps;

public class GetPageRoadmapsQueryHandler : IRequestHandler<GetPageRoadmapsQuery, ValueTask<Result<PagedResponse<RoadmapPageItemResponse>>>>
{
    private readonly IRoadmapRepository _roadmapRepository;

    public GetPageRoadmapsQueryHandler(
        IRoadmapRepository roadmapRepository)
    {
        _roadmapRepository = roadmapRepository;
    }

    public async ValueTask<Result<PagedResponse<RoadmapPageItemResponse>>> Handle(GetPageRoadmapsQuery request, CancellationToken cancellationToken)
    {
        var (roadmaps, totalCount) = await _roadmapRepository.GetPageByUserIdAsync(
            request.UserId,
            request.PageNumber,
            request.PageSize,
            cancellationToken);

        var items = roadmaps.Select(r => new RoadmapPageItemResponse(
            r.Id,
            r.Name,
            r.Description,
            r.ResourceId,
            r.CreatedAt
        )).ToList();

        return Result.Ok(new PagedResponse<RoadmapPageItemResponse>(items, totalCount, request.PageNumber, request.PageSize));
    }
}
