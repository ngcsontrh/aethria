namespace Aethria.Api.Endpoints.Chat;

/// <summary>
/// Available AI tool response.
/// </summary>
public sealed record GetAvailableToolsResponse
{
    /// <summary>
    /// Tool identifier used in chat requests.
    /// </summary>
    public string Id { get; init; } = null!;

    /// <summary>
    /// Human-readable tool name.
    /// </summary>
    public string Name { get; init; } = null!;

    /// <summary>
    /// Short description of the tool capability.
    /// </summary>
    public string Description { get; init; } = null!;
}

