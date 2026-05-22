namespace Aethria.Api.Endpoints.Chat;

/// <summary>
/// Chat history response containing all messages in the session.
/// </summary>
public sealed record GetChatHistoryResponse
{
    /// <summary>
    /// All chat messages in the session.
    /// </summary>
    public IReadOnlyList<GetChatMessageItemResponse> Messages { get; init; } = [];
}

/// <summary>
/// Chat message list item response.
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
