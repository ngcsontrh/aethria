using Aethria.Application.UseCases.Roadmaps.Events;
using Microsoft.Extensions.Logging;

namespace Aethria.Application.UseCases.Roadmaps.GenerateAIRoadmap;

public sealed class GenerateAIRoadmapCommandHandler : IRequestHandler<GenerateAIRoadmapCommand, ValueTask<Result<Guid>>>
{
    private readonly IResourceRepository _resourceRepository;
    private readonly IRoadmapRepository _roadmapRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMediator _mediator;
    private readonly IAIRoadmapGenerationAgent _agent;
    private readonly ILogger<GenerateAIRoadmapCommandHandler> _logger;

    public GenerateAIRoadmapCommandHandler(
        IResourceRepository resourceRepository,
        IRoadmapRepository roadmapRepository,
        IUnitOfWork unitOfWork,
        IMediator mediator,
        IAIRoadmapGenerationAgent agent,
        ILogger<GenerateAIRoadmapCommandHandler> logger)
    {
        _resourceRepository = resourceRepository;
        _roadmapRepository = roadmapRepository;
        _unitOfWork = unitOfWork;
        _mediator = mediator;
        _agent = agent;
        _logger = logger;
    }

    public async ValueTask<Result<Guid>> Handle(
        GenerateAIRoadmapCommand command,
        CancellationToken cancellationToken)
    {
        var resource = await _resourceRepository.GetByIdAsync(command.ResourceId, cancellationToken);
        if (resource is null || resource.UserId != command.UserId)
        {
            return Result.Fail<Guid>(new NotFoundError("Resource not found."));
        }

        if (string.IsNullOrWhiteSpace(resource.Content))
        {
            const string message = "Resource has no content for AI roadmap generation.";
            return Result.Fail<Guid>(new ValidationError(message));
        }

        var input = new GenerateAIRoadmapInput
        {
            SourceContent = resource.Content,
            UserPrompt = command.Prompt
        };

        GenerateAIRoadmapResult? completedResult = null;

        await foreach (var result in _agent.RunAsync(input, cancellationToken))
        {
            if (result.IsFailed)
            {
                var message = result.ErrorMessage ?? result.Message ?? "AI roadmap generation failed.";
                return Result.Fail<Guid>(new InternalError(message));
            }

            if (result.IsCompleted)
            {
                completedResult = result;
                break;
            }

            _logger.LogDebug(
                "AI roadmap generation progress for resource {ResourceId}: {Status} - {Message}",
                command.ResourceId,
                result.Status,
                result.Message);
        }

        if (completedResult is null)
        {
            const string message = "AI roadmap generation failed.";
            return Result.Fail<Guid>(new InternalError(message));
        }

        if (string.IsNullOrWhiteSpace(completedResult.RoadmapContent))
        {
            const string message = "AI generated an empty roadmap.";
            return Result.Fail<Guid>(new InternalError(message));
        }

        var roadmapId = Guid.CreateVersion7();
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

        return Result.Ok(roadmapId);
    }
}
