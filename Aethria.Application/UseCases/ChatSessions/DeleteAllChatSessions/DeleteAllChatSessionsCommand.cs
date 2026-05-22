namespace Aethria.Application.UseCases.ChatSessions.DeleteAllChatSessions;

public sealed record DeleteAllChatSessionsCommand(
    Guid UserId,
    ChatSessionScope Scope = ChatSessionScope.General,
    Guid? ScopeId = null) : IRequest<DeleteAllChatSessionsCommand, ValueTask<Result>>;
