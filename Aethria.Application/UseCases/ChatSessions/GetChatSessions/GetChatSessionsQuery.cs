namespace Aethria.Application.UseCases.ChatSessions.GetChatSessions;

public sealed record GetChatSessionsQuery(
    Guid UserId,
    int PageNumber,
    int PageSize,
    ChatSessionScope Scope = ChatSessionScope.General,
    Guid? ScopeId = null) : IRequest<GetChatSessionsQuery, ValueTask<Result<PagedResponse<ChatSessionItemResponse>>>>;
