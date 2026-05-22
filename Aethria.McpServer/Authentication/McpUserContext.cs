using ModelContextProtocol;

namespace Aethria.McpServer.Authentication;

internal static class McpUserContext
{
    public static Guid GetRequiredUserId(ClaimsPrincipal? user)
    {
        var userIdString = user?.FindFirstValue(ClaimTypes.NameIdentifier);
        if (Guid.TryParse(userIdString, out var userId))
        {
            return userId;
        }

        throw new McpException("Authenticated MCP user id claim is missing.");
    }
}
