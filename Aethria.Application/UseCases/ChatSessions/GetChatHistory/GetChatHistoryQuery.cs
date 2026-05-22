namespace Aethria.Application.UseCases.ChatSessions.GetChatHistory;

public sealed record GetChatHistoryQuery(
    Guid SessionId,
    Guid UserId,
    ChatSessionScope Scope = ChatSessionScope.General,
    Guid? ScopeId = null) : IRequest<GetChatHistoryQuery, ValueTask<Result<GetChatHistoryResponse>>>;
