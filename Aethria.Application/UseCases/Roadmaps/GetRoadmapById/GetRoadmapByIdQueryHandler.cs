namespace Aethria.Application.UseCases.Roadmaps.GetRoadmapById;

public class GetRoadmapByIdQueryHandler : IRequestHandler<GetRoadmapByIdQuery, ValueTask<Result<RoadmapDetailResponse>>>
{
    private readonly IRoadmapRepository _roadmapRepository;

    public GetRoadmapByIdQueryHandler(
        IRoadmapRepository roadmapRepository)
    {
        _roadmapRepository = roadmapRepository;
    }

    public async ValueTask<Result<RoadmapDetailResponse>> Handle(GetRoadmapByIdQuery request, CancellationToken cancellationToken)
    {
        var roadmap = await _roadmapRepository.GetByIdAsync(request.RoadmapId, cancellationToken);

        if (roadmap == null || roadmap.UserId != request.UserId)
        {
            return Result.Fail(new NotFoundError("Roadmap not found."));
        }

        var response = new RoadmapDetailResponse(
            roadmap.Id,
            roadmap.Name,
            roadmap.Description,
            roadmap.Content,
            roadmap.Mermaid,
            roadmap.ResourceId,
            roadmap.CreatedAt);

        return Result.Ok(response);
    }
}
