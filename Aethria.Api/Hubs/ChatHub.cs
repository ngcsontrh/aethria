using Aethria.Api.Endpoints.Chat;
using Aethria.Api.Endpoints.Mentors;
using Aethria.Api.Endpoints.Resources;
using Aethria.Application.UseCases.Chat.Contracts;
using Aethria.Application.UseCases.Chat.GeneralChat;
using Aethria.Application.UseCases.Chat.MentorChat;
using Aethria.Application.UseCases.Chat.ResourceChat;
using Microsoft.AspNetCore.SignalR;
using System.Runtime.CompilerServices;

namespace Aethria.Api.Hubs;

/// <summary>
/// Streams AI chat responses over SignalR.
/// </summary>
[Authorize]
public sealed class ChatHub : Hub
{
    private readonly IMediator _mediator;

    /// <summary>
    /// Initializes a new instance of the <see cref="ChatHub"/> class.
    /// </summary>
    public ChatHub(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Stream a response from the general AI chat agent.
    /// </summary>
    public async IAsyncEnumerable<ChatStreamResponse> GeneralChatStream(
        ChatRequest request,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var command = new GeneralChatCommand(
            Message: request.Message,
            SessionId: request.SessionId,
            Tools: request.Tools ?? [],
            UserId: Context.User!.GetUserId());

        await foreach (var streamEvent in StreamChatResultsAsync(
                           _mediator.CreateStream(command, cancellationToken),
                           cancellationToken))
        {
            yield return streamEvent;
        }
    }

    /// <summary>
    /// Stream a chat response from a specific mentor.
    /// </summary>
    public async IAsyncEnumerable<ChatStreamResponse> MentorChatStream(
        Guid mentorId,
        MentorChatRequest request,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var command = new MentorChatCommand(
            Message: request.Message,
            MentorId: mentorId,
            SessionId: request.SessionId,
            UserId: Context.User!.GetUserId());

        await foreach (var streamEvent in StreamChatResultsAsync(
                           _mediator.CreateStream(command, cancellationToken),
                           cancellationToken))
        {
            yield return streamEvent;
        }
    }

    /// <summary>
    /// Stream a chat response about a specific resource.
    /// </summary>
    public async IAsyncEnumerable<ChatStreamResponse> ResourceChatStream(
        Guid resourceId,
        ResourceChatRequest request,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var command = new ResourceChatCommand(
            ResourceId: resourceId,
            Message: request.Message,
            SessionId: request.SessionId,
            UserId: Context.User!.GetUserId());

        await foreach (var streamEvent in StreamChatResultsAsync(
                           _mediator.CreateStream(command, cancellationToken),
                           cancellationToken))
        {
            yield return streamEvent;
        }
    }

    private static async IAsyncEnumerable<ChatStreamResponse> StreamChatResultsAsync(
        IAsyncEnumerable<Result<ChatStreamResponse>> results,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        await using var enumerator = results.GetAsyncEnumerator(cancellationToken);

        while (true)
        {
            Result<ChatStreamResponse> result;
            ChatStreamResponse? failureEvent = null;
            var hasNext = false;

            try
            {
                hasNext = await enumerator.MoveNextAsync();
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                yield break;
            }
            catch (OperationCanceledException)
            {
                failureEvent = CreateFailureEvent("Chat response timed out. Please try again.");
            }
            catch (Exception)
            {
                failureEvent = CreateFailureEvent("AI service is temporarily unavailable. Please try again later.");
            }

            if (failureEvent is not null)
            {
                yield return failureEvent;
                yield break;
            }

            if (!hasNext)
            {
                yield break;
            }

            result = enumerator.Current;

            var streamEvent = result.IsSuccess
                ? result.Value
                : CreateFailureEvent(result.ToStreamFailureMessage("Chat stream failed."));

            yield return streamEvent;

            if (streamEvent.Status == ChatStreamResponse.Statuses.Failed)
            {
                yield break;
            }
        }
    }

    private static ChatStreamResponse CreateFailureEvent(string message)
    {
        return new ChatStreamResponse(ChatStreamResponse.Statuses.Failed, Message: message);
    }
}
