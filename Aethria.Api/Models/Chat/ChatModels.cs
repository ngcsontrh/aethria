using System.ComponentModel;

namespace Aethria.Api.Endpoints.Chat;

/// <summary>
/// General chat request.
/// </summary>
public sealed record ChatRequest
{
    /// <summary>
    /// User message to send to the assistant.
    /// </summary>
    [Required(ErrorMessage = "Message is required.")]
    [StringLength(4000, MinimumLength = 1, ErrorMessage = "Message must be between 1 and 4000 characters.")]
    public string Message { get; init; } = null!;

    /// <summary>
    /// Existing chat session identifier, if continuing a session.
    /// </summary>
    public Guid? SessionId { get; init; }

    /// <summary>
    /// Optional tool identifiers enabled for this chat turn.
    /// </summary>
    [Description("Valid values: web_search, web_extract")]
    [MaxLength(20, ErrorMessage = "Tools must contain 20 items or fewer.")]
    public List<string>? Tools { get; init; }
}
