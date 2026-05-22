using System.ComponentModel;
using System.Text.Json.Serialization;

namespace Aethria.Infrastructure.AgentFramework.Mentors;

/// <summary>
/// AI structured output for mentor instruction validation.
/// </summary>
internal sealed class MentorInstructionValidationResponse
{
    [JsonPropertyName("outcome")]
    [Description("'valid' or 'invalid'.")]
    public string Outcome { get; set; } = null!;

    [JsonPropertyName("reason")]
    [Description("Reason for rejection. Empty string if valid.")]
    public string Reason { get; set; } = null!;
}
