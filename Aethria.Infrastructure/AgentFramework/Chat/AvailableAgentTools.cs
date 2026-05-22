namespace Aethria.Infrastructure.AgentFramework.Chat;

public sealed class AgentToolInfo
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public required string Description { get; init; }
}

public static class AvailableAgentTools
{
    public const string WebSearchId = "web_search";
    public const string WebExtractId = "web_extract";

    public static readonly IReadOnlyList<AgentToolInfo> Tools =
    [
        new()
        {
            Id = WebSearchId,
            Name = "Web Search",
            Description = "Search for real-time information on the Internet."
        },
        new()
        {
            Id = WebExtractId,
            Name = "Web Extract",
            Description = "Read and extract content from a specific web page."
        }
    ];
}
