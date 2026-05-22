using Aethria.Api.Endpoints.Roadmaps;
using Aethria.Application.UseCases.Roadmaps.GenerateAIRoadmapStream;
using Microsoft.AspNetCore.SignalR;
using System.Runtime.CompilerServices;

namespace Aethria.Api.Hubs;

/// <summary>
/// Streams roadmap generation progress over SignalR.
/// </summary>
[Authorize]
public sealed class RoadmapHub : Hub
{
    private readonly IMediator _mediator;

    /// <summary>
    /// Initializes a new instance of the <see cref="RoadmapHub"/> class.
    /// </summary>
    public RoadmapHub(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Generate a roadmap with AI and stream generation progress.
    /// </summary>
    public async IAsyncEnumerable<GenerateAIRoadmapStreamEvent> GenerateAIRoadmapStream(
        GenerateAIRoadmapRequest request,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var command = new GenerateAIRoadmapStreamCommand(
            Name: request.Name,
            Description: request.Description,
            ResourceId: request.ResourceId,
            Prompt: request.Prompt,
            UserId: Context.User!.GetUserId());

        await foreach (var result in _mediator.CreateStream(command, cancellationToken).WithCancellation(cancellationToken))
        {
            yield return result.IsSuccess
                ? result.Value
                : new GenerateAIRoadmapStreamEvent(
                    Status: GenerateAIRoadmapStreamResult.Statuses.Failed,
                    Message: result.ToStreamFailureMessage("AI roadmap generation failed."));
        }
    }
}
