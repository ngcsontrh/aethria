using Aethria.Application.UseCases.Chat.Contracts;
using DispatchR.Abstractions.Stream;
namespace Aethria.Application.UseCases.Chat.ResourceChat;

public sealed record ResourceChatCommand(
    Guid ResourceId,
    string Message,
    Guid? SessionId,
    Guid UserId) : IStreamRequest<ResourceChatCommand, Result<ChatStreamResponse>>;
