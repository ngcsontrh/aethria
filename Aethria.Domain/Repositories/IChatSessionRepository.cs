namespace Aethria.Domain.Repositories;

public interface IChatSessionRepository
{
    Task AddAsync(ChatSession chatSession, CancellationToken cancellationToken);
    Task UpdateAsync(ChatSession chatSession, CancellationToken cancellationToken);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken);
    Task<ChatSession?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task DeleteAllByUserIdAsync(Guid userId, CancellationToken cancellationToken);
    Task DeleteAllGeneralByUserIdAsync(Guid userId, CancellationToken cancellationToken);
    Task DeleteAllByMentorIdAsync(Guid mentorId, Guid userId, CancellationToken cancellationToken);
    Task DeleteAllByResourceIdAsync(Guid resourceId, Guid userId, CancellationToken cancellationToken);
    Task AddMessagesAsync(IEnumerable<ChatMessage> messages, CancellationToken cancellationToken);
    Task<IReadOnlyList<ChatMessage>> ListMessagesByChatSessionIdAsync(Guid chatSessionId, CancellationToken cancellationToken);
    Task<IReadOnlyList<ChatMessage>> ListUserVisibleMessagesByChatSessionIdAsync(Guid chatSessionId, CancellationToken cancellationToken);
    Task<(IReadOnlyList<ChatSession> ChatSessions, int TotalCount)> GetPageByMentorIdAsync(Guid mentorId, Guid userId, int pageNumber, int pageSize, CancellationToken cancellationToken);
    Task<(IReadOnlyList<ChatSession> ChatSessions, int TotalCount)> GetPageByResourceIdAsync(Guid resourceId, Guid userId, int pageNumber, int pageSize, CancellationToken cancellationToken);
    Task<(IReadOnlyList<ChatSession> ChatSessions, int TotalCount)> GetPageGeneralByUserIdAsync(Guid userId, int pageNumber, int pageSize, CancellationToken cancellationToken);
}
