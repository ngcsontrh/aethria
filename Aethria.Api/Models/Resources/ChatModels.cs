namespace Aethria.Api.Endpoints.Resources;

/// <summary>
/// Resource chat request.
/// </summary>
public sealed record ResourceChatRequest
{
    /// <summary>
    /// User message to ask about the resource.
    /// </summary>
    [Required(ErrorMessage = "Message is required.")]
    [StringLength(2000, MinimumLength = 1, ErrorMessage = "Message must be between 1 and 2000 characters.")]
    public string Message { get; init; } = null!;

    /// <summary>
    /// Existing resource chat session identifier, if continuing a session.
    /// </summary>
    public Guid? SessionId { get; init; }
}

