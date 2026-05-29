using Aethria.Api.Endpoints.Chat;
using Aethria.Infrastructure.AgentFramework.Chat;

namespace Aethria.Api.Endpoints;

internal static class ChatEndpoints
{
    internal static void MapChatEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/chat")
            .RequireAuthorization()
            .WithTags("Chat");

        group.MapGet("tools", GetAvailableTools)
            .WithName("GetAvailableChatTools")
            .Produces<List<GetAvailableToolsResponse>>(StatusCodes.Status200OK);
    }

    /// <summary>
    /// Get the list of available tools that the AI agent can use.
    /// </summary>
    public static IResult GetAvailableTools()
    {
        var tools = AvailableAgentTools.Tools.Select(tool => new GetAvailableToolsResponse
        {
            Id = tool.Id,
            Name = tool.Name,
            Description = tool.Description
        }).ToList();

        return Results.Ok(tools);
    }
}
