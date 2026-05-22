namespace Aethria.Api.Endpoints.ChatSessions;

/// <summary>
/// Query parameters for getting paginated chat sessions.
/// </summary>
public sealed record GetChatSessionsRequest
{
    /// <summary>
    /// Page number to retrieve.
    /// </summary>
    [Range(1, int.MaxValue, ErrorMessage = "Page number must be greater than or equal to 1.")]
    public int PageNumber { get; init; } = 1;

    /// <summary>
    /// Number of items per page.
    /// </summary>
    [Range(1, 100, ErrorMessage = "Page size must be between 1 and 100.")]
    public int PageSize { get; init; } = 20;
}



/// <summary>
/// Chat session list item response.
/// </summary>
public sealed record GetChatSessionItemResponse
{
    /// <summary>
    /// Chat session identifier.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// Chat session display name.
    /// </summary>
    public string Name { get; init; } = null!;

    /// <summary>
    /// Optional chat session description.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Time when the chat session was created.
    /// </summary>
    public DateTimeOffset CreatedAt { get; init; }

    /// <summary>
    /// Time when the chat session was last updated.
    /// </summary>
    public DateTimeOffset UpdatedAt { get; init; }
}
