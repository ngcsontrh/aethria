namespace Aethria.Api.Endpoints.Mentors;

/// <summary>
/// Mentor chat request.
/// </summary>
public sealed record MentorChatRequest
{
    /// <summary>
    /// User message to send to the mentor.
    /// </summary>
    [Required(ErrorMessage = "Message is required.")]
    [StringLength(4000, MinimumLength = 1, ErrorMessage = "Message must be between 1 and 4000 characters.")]
    public string Message { get; init; } = string.Empty;

    /// <summary>
    /// Existing mentor chat session identifier, if continuing a session.
    /// </summary>
    public Guid? SessionId { get; init; }
}
