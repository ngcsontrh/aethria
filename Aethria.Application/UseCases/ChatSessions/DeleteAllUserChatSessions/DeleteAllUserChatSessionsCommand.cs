namespace Aethria.Application.UseCases.ChatSessions.DeleteAllUserChatSessions;

public sealed record DeleteAllUserChatSessionsCommand(Guid UserId) : IRequest<DeleteAllUserChatSessionsCommand, ValueTask<Result>>;
