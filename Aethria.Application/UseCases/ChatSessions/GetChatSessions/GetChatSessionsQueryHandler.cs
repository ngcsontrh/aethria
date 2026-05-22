namespace Aethria.Application.UseCases.ChatSessions.GetChatSessions;

public sealed class GetChatSessionsQueryHandler : IRequestHandler<GetChatSessionsQuery, ValueTask<Result<PagedResponse<ChatSessionItemResponse>>>>
{
    private readonly IChatSessionRepository _chatSessionRepository;

    public GetChatSessionsQueryHandler(
        IChatSessionRepository chatSessionRepository)
    {
        _chatSessionRepository = chatSessionRepository;
    }

    public async ValueTask<Result<PagedResponse<ChatSessionItemResponse>>> Handle(GetChatSessionsQuery query, CancellationToken cancellationToken)
    {
        var (sessions, totalCount) = query.Scope switch
        {
            ChatSessionScope.General => await _chatSessionRepository.GetPageGeneralByUserIdAsync(
                query.UserId, query.PageNumber, query.PageSize, cancellationToken),
            ChatSessionScope.Mentor => await _chatSessionRepository.GetPageByMentorIdAsync(
                query.ScopeId!.Value, query.UserId, query.PageNumber, query.PageSize, cancellationToken),
            ChatSessionScope.Resource => await _chatSessionRepository.GetPageByResourceIdAsync(
                query.ScopeId!.Value, query.UserId, query.PageNumber, query.PageSize, cancellationToken),
            _ => throw new InvalidOperationException($"Unsupported chat session scope '{query.Scope}'.")
        };

        var items = sessions.Select(s => new ChatSessionItemResponse(
            Id: s.Id,
            Name: s.Name,
            Description: s.Description,
            CreatedAt: s.CreatedAt,
            UpdatedAt: s.UpdatedAt
        )).ToList();

        return Result.Ok(new PagedResponse<ChatSessionItemResponse>(items, totalCount, query.PageNumber, query.PageSize));
    }
}
