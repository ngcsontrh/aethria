namespace Aethria.Application.UseCases.Chat.Contracts;

public sealed record ChatAgentStreamResult(
    string? Delta = null,
    string? Answer = null,
    IReadOnlyList<(string Role, string Content)>? Messages = null,
    bool IsCompleted = false);
