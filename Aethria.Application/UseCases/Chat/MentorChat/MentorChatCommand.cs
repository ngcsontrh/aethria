using Aethria.Application.UseCases.Chat.Contracts;
using DispatchR.Abstractions.Stream;
namespace Aethria.Application.UseCases.Chat.MentorChat;

public sealed record MentorChatCommand(
    string Message,
    Guid MentorId,
    Guid? SessionId,
    Guid UserId) : IStreamRequest<MentorChatCommand, Result<ChatStreamResponse>>;
