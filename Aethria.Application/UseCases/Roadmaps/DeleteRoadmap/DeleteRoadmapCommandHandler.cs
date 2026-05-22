namespace Aethria.Application.UseCases.Roadmaps.DeleteRoadmap;

public class DeleteRoadmapCommandHandler : IRequestHandler<DeleteRoadmapCommand, ValueTask<Result>>
{
    private readonly IRoadmapRepository _roadmapRepository;

    public DeleteRoadmapCommandHandler(
        IRoadmapRepository roadmapRepository)
    {
        _roadmapRepository = roadmapRepository;
    }

    public async ValueTask<Result> Handle(DeleteRoadmapCommand command, CancellationToken cancellationToken)
    {
        var roadmap = await _roadmapRepository.GetByIdAsync(command.RoadmapId, cancellationToken);

        if (roadmap == null || roadmap.UserId != command.UserId)
        {
            return Result.Fail(new NotFoundError("Roadmap not found."));
        }

        await _roadmapRepository.DeleteAsync(command.RoadmapId, cancellationToken);

        return Result.Ok();
    }
}
