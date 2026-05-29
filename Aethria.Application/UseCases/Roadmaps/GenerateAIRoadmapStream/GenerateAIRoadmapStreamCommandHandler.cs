using Aethria.Application.UseCases.Roadmaps.Events;
using System.Runtime.CompilerServices;

namespace Aethria.Application.UseCases.Roadmaps.GenerateAIRoadmapStream;

public sealed class GenerateAIRoadmapStreamCommandHandler : IStreamRequestHandler<GenerateAIRoadmapStreamCommand, Result<GenerateAIRoadmapStreamEvent>>
{
    private readonly IResourceRepository _resourceRepository;
    private readonly IRoadmapRepository _roadmapRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMediator _mediator;
    private readonly IAIRoadmapGenerationAgent _agent;

    public GenerateAIRoadmapStreamCommandHandler(
        IResourceRepository resourceRepository,
        IRoadmapRepository roadmapRepository,
        IUnitOfWork unitOfWork,
        IMediator mediator,
        IAIRoadmapGenerationAgent agent)
    {
        _resourceRepository = resourceRepository;
        _roadmapRepository = roadmapRepository;
        _unitOfWork = unitOfWork;
        _mediator = mediator;
        _agent = agent;
    }

    public async IAsyncEnumerable<Result<GenerateAIRoadmapStreamEvent>> Handle(
        GenerateAIRoadmapStreamCommand command,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var resource = await _resourceRepository.GetByIdAsync(command.ResourceId, cancellationToken);
        if (resource is null || resource.UserId != command.UserId)
        {
            yield return Result.Fail<GenerateAIRoadmapStreamEvent>(new NotFoundError("Resource not found."));
            yield break;
        }

        if (string.IsNullOrWhiteSpace(resource.Content))
        {
            yield return Result.Fail<GenerateAIRoadmapStreamEvent>(
                new ValidationError("Resource has no content for AI roadmap generation."));
            yield break;
        }

        var input = new GenerateAIRoadmapStreamInput
        {
            SourceContent = resource.Content,
            UserPrompt = command.Prompt
        };

        GenerateAIRoadmapStreamResult? completedResult = null;

        await foreach (var result in _agent.RunAsync(input, cancellationToken))
        {
            if (result.IsFailed)
            {
                yield return Result.Fail<GenerateAIRoadmapStreamEvent>(
                    new InternalError(result.ErrorMessage ?? result.Message ?? "AI roadmap generation failed."));
                yield break;
            }

            if (result.IsCompleted)
            {
                completedResult = result;
                break;
            }

            yield return Result.Ok(new GenerateAIRoadmapStreamEvent(
                Status: result.Status,
                Message: result.Message ?? "AI roadmap generation is in progress."));
        }

        if (completedResult is null)
        {
            yield return Result.Fail<GenerateAIRoadmapStreamEvent>(new InternalError("AI roadmap generation failed."));
            yield break;
        }

        if (string.IsNullOrWhiteSpace(completedResult.RoadmapContent))
        {
            yield return Result.Fail<GenerateAIRoadmapStreamEvent>(new InternalError("AI generated an empty roadmap."));
            yield break;
        }

        var roadmapId = Guid.NewGuid();
        var now = DateTimeOffset.UtcNow;
        var roadmap = new Roadmap
        {
            Id = roadmapId,
            UserId = command.UserId,
            Name = command.Name,
            Description = command.Description,
            Content = completedResult.RoadmapContent,
            Mermaid = completedResult.MermaidDiagram,
            ResourceId = command.ResourceId,
            CreatedAt = now,
            UpdatedAt = now
        };

        await _roadmapRepository.AddAsync(roadmap, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var completedEvent = new AIRoadmapGenerationCompletedEvent(
            RoadmapId: roadmapId,
            UserId: command.UserId);

        await _mediator.Publish(completedEvent, cancellationToken);

        yield return Result.Ok(new GenerateAIRoadmapStreamEvent(
            Status: GenerateAIRoadmapStreamResult.Statuses.Completed,
            Message: "AI roadmap generation completed.",
            RoadmapId: roadmapId));
    }
}
