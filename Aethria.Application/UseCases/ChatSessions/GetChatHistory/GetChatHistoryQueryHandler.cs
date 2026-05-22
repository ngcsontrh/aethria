namespace Aethria.Application.UseCases.ChatSessions.GetChatHistory;

public sealed class GetChatHistoryQueryHandler : IRequestHandler<GetChatHistoryQuery, ValueTask<Result<GetChatHistoryResponse>>>
{
    private readonly IChatSessionRepository _chatSessionRepository;

    public GetChatHistoryQueryHandler(
        IChatSessionRepository chatSessionRepository)
    {
        _chatSessionRepository = chatSessionRepository;
    }

    public async ValueTask<Result<GetChatHistoryResponse>> Handle(GetChatHistoryQuery query, CancellationToken cancellationToken)
    {
        var session = await _chatSessionRepository.GetByIdAsync(query.SessionId, cancellationToken);
        if (session is null)
        {
            return Result.Fail(new NotFoundError("ChatSession", query.SessionId.ToString()));
        }

        if (session.UserId != query.UserId || !SessionMatchesScope(session.MentorId, session.ResourceId, query.Scope, query.ScopeId))
        {
            return Result.Fail(new NotFoundError("ChatSession", query.SessionId.ToString()));
        }

        var messages = await _chatSessionRepository.ListUserVisibleMessagesByChatSessionIdAsync(
            query.SessionId, cancellationToken);

        var items = messages.Select(m => new ChatMessageResponse(
            Id: m.Id,
            Role: m.Role.Value,
            Content: m.Content,
            CreatedAt: m.CreatedAt
        )).ToList();

        return Result.Ok(new GetChatHistoryResponse(Messages: items));
    }

    private static bool SessionMatchesScope(Guid? mentorId, Guid? resourceId, ChatSessionScope scope, Guid? scopeId)
    {
        return scope switch
        {
            ChatSessionScope.General => mentorId is null && resourceId is null,
            ChatSessionScope.Mentor => mentorId == scopeId,
            ChatSessionScope.Resource => resourceId == scopeId,
            _ => false
        };
    }
}
