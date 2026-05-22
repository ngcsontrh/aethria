using Aethria.Application.UseCases.Chat.Contracts;
using DispatchR.Abstractions.Stream;
namespace Aethria.Application.UseCases.Chat.GeneralChat;

public sealed record GeneralChatCommand(
    string Message,
    Guid? SessionId,
    IReadOnlyList<string> Tools,
    Guid UserId) : IStreamRequest<GeneralChatCommand, Result<ChatStreamResponse>>;
