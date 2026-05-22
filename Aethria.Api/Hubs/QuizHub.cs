using Aethria.Api.Endpoints.Quizzes;
using Aethria.Application.UseCases.Quizzes.CreateAIQuizStream;
using Microsoft.AspNetCore.SignalR;
using System.Runtime.CompilerServices;

namespace Aethria.Api.Hubs;

/// <summary>
/// Streams quiz generation progress over SignalR.
/// </summary>
[Authorize]
public sealed class QuizHub : Hub
{
    private readonly IMediator _mediator;

    /// <summary>
    /// Initializes a new instance of the <see cref="QuizHub"/> class.
    /// </summary>
    public QuizHub(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Create a new quiz with AI-generated questions and stream generation progress.
    /// </summary>
    public async IAsyncEnumerable<CreateAIQuizStreamEvent> CreateAIQuizStream(
        CreateAIQuizRequest request,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var command = new CreateAIQuizStreamCommand(
            Name: request.Name,
            Description: request.Description,
            ResourceId: request.ResourceId,
            Prompt: request.Prompt,
            NumberOfQuestions: request.NumberOfQuestions,
            UserId: Context.User!.GetUserId());

        await foreach (var result in _mediator.CreateStream(command, cancellationToken).WithCancellation(cancellationToken))
        {
            yield return result.IsSuccess
                ? result.Value
                : new CreateAIQuizStreamEvent(
                    Status: CreateAIQuizStreamResult.Statuses.Failed,
                    Message: result.ToStreamFailureMessage("AI quiz generation failed."));
        }
    }
}
