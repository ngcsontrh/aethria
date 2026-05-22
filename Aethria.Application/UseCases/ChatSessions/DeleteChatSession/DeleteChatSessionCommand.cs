namespace Aethria.Application.UseCases.ChatSessions.DeleteChatSession;

public sealed record DeleteChatSessionCommand(
    Guid SessionId,
    Guid UserId,
    ChatSessionScope Scope = ChatSessionScope.General,
    Guid? ScopeId = null) : IRequest<DeleteChatSessionCommand, ValueTask<Result>>;
