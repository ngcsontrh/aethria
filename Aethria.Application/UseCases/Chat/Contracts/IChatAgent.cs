namespace Aethria.Application.UseCases.Chat.Contracts;

public sealed record ChatAgentContext(
    IReadOnlyList<(string Role, string Content)> Messages,
    string? Instruction = null,
    IReadOnlyList<string>? Tools = null,
    Guid? ResourceId = null);

public interface IChatAgent
{
    IAsyncEnumerable<ChatAgentStreamResult> RunStreamingAsync(
        ChatAgentContext context,
        CancellationToken cancellationToken);
}
