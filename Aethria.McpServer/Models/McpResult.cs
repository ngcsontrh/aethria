using System.Text.Json.Serialization;

namespace Aethria.McpServer.Models;

public sealed record McpResult
{
    [JsonPropertyName("state")]
    public string State { get; init; }

    [JsonPropertyName("result")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public object? Result { get; init; }

    [JsonPropertyName("errorMessage")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ErrorMessage { get; init; }

    private McpResult(string state, object? result = null, string? errorMessage = null)
    {
        State = state;
        Result = result;
        ErrorMessage = errorMessage;
    }

    public static McpResult Success(object? result) => new McpResult("success", result);

    public static McpResult Error(string errorMessage) => new McpResult("error", null, errorMessage);
}
