using Aethria.Application.UseCases.Chat.Contracts;
namespace Aethria.Application.UseCases.Chat.GeneralChat;

public sealed record GeneralChatCommand(
    string Message,
    Guid? SessionId,
    IReadOnlyList<string> Tools,
    Guid UserId) : IStreamRequest<GeneralChatCommand, Result<ChatStreamResponse>>;
