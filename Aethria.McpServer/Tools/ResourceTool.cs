using Aethria.Application.UseCases.Chat.Contracts;
using Aethria.Application.UseCases.Chat.ResourceChat;
using Aethria.Application.UseCases.Resources.GetResourceSelector;
using Aethria.McpServer.Extensions;
using System.ComponentModel;

namespace Aethria.McpServer.Tools;

[Authorize]
[McpServerToolType]
public class ResourceTool
{
    private readonly IMediator _mediator;

    public ResourceTool(IMediator mediator)
    {
        _mediator = mediator;
    }

    [McpServerTool, Description("List the current user's learning resources so the caller can pick a resource ID to chat with.")]
    public async Task<McpResult> GetBasicList(
        ClaimsPrincipal? user,
        CancellationToken cancellationToken)
    {
        var userId = McpUserContext.GetRequiredUserId(user);
        var result = await _mediator.Send(
            new GetResourceSelectorQuery(userId),
            cancellationToken);

        return result.ToMcpResult();
    }

    [McpServerTool, Description("Chat with one of the user's resources by resource ID.")]
    public async Task<McpResult> ChatResource(
        ClaimsPrincipal? user,
        CancellationToken cancellationToken,
        [Description("The resource ID from the resource list.")] Guid resourceId,
        [Description("The user's message for this resource.")] string message,
        [Description("Optional existing resource chat session ID to continue.")] Guid? sessionId = null)
    {
        var userId = McpUserContext.GetRequiredUserId(user);
        ChatStreamResponse? finalEvent = null;
        await foreach (var streamResult in _mediator.CreateStream(
            new ResourceChatCommand(resourceId, message, sessionId, userId),
            cancellationToken))
        {
            if (streamResult.IsFailed)
            {
                return streamResult.ToMcpErrorResult();
            }

            if (streamResult.Value.Status == ChatStreamResponse.Statuses.Completed)
            {
                finalEvent = streamResult.Value;
            }
        }

        if (finalEvent is null)
        {
            return McpResult.Error("Resource chat stream completed without a final result.");
        }

        return McpResult.Success(finalEvent);
    }
}
