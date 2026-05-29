using Aethria.Application.UseCases.Chat.Contracts;
namespace Aethria.Application.UseCases.Chat.ResourceChat;

public sealed record ResourceChatCommand(
    Guid ResourceId,
    string Message,
    Guid? SessionId,
    Guid UserId) : IStreamRequest<ResourceChatCommand, Result<ChatStreamResponse>>;
