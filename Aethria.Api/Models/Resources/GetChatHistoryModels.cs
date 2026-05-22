namespace Aethria.Api.Endpoints.Resources;

/// <summary>
/// Resource chat history response.
/// </summary>
public sealed record GetChatHistoryResponse
{
    /// <summary>
    /// Chat messages in the session.
    /// </summary>
    public IReadOnlyList<GetChatMessageItemResponse> Messages { get; init; } = [];
}

/// <summary>
/// Resource chat message list item response.
/// </summary>
public sealed record GetChatMessageItemResponse
{
    /// <summary>
    /// Chat message identifier.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// Message role, such as user or assistant.
    /// </summary>
    public string Role { get; init; } = null!;

    /// <summary>
    /// Message content.
    /// </summary>
    public string Content { get; init; } = null!;

    /// <summary>
    /// Time when the message was created.
    /// </summary>
    public DateTimeOffset CreatedAt { get; init; }
}
