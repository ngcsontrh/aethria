namespace Aethria.Infrastructure.Repositories;

internal class ChatSessionRepository : IChatSessionRepository
{
    private readonly AppDbContext _dbContext;

    public ChatSessionRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task AddAsync(ChatSession chatSession, CancellationToken cancellationToken)
    {
        _dbContext.ChatSessions.Add(chatSession);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(ChatSession chatSession, CancellationToken cancellationToken)
    {
        var entry = _dbContext.Entry(chatSession);
        if (entry.State == EntityState.Detached)
        {
            _dbContext.ChatSessions.Attach(chatSession);
            entry.State = EntityState.Modified;
        }
        return Task.CompletedTask;
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var session = await _dbContext.ChatSessions.FindAsync([id], cancellationToken);
        if (session != null)
        {
            _dbContext.ChatSessions.Remove(session);
        }
    }

    public async Task<ChatSession?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _dbContext.ChatSessions
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    public async Task DeleteAllByUserIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        var sessionIds = await _dbContext.ChatSessions
            .Where(s => s.UserId == userId)
            .Select(s => s.Id)
            .ToListAsync(cancellationToken);

        if (sessionIds.Count == 0)
        {
            return;
        }

        var messages = await _dbContext.ChatMessages
            .Where(m => sessionIds.Contains(m.SessionId))
            .ToListAsync(cancellationToken);

        _dbContext.ChatMessages.RemoveRange(messages);

        var sessions = await _dbContext.ChatSessions
            .Where(s => sessionIds.Contains(s.Id))
            .ToListAsync(cancellationToken);

        _dbContext.ChatSessions.RemoveRange(sessions);
    }

    public async Task DeleteAllGeneralByUserIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        var sessions = await _dbContext.ChatSessions
            .Where(s => s.UserId == userId && s.MentorId == null && s.ResourceId == null)
            .ToListAsync(cancellationToken);

        _dbContext.ChatSessions.RemoveRange(sessions);
    }

    public async Task DeleteAllByMentorIdAsync(Guid mentorId, Guid userId, CancellationToken cancellationToken)
    {
        var sessionIds = await _dbContext.ChatSessions
            .Where(s => s.MentorId == mentorId && s.UserId == userId)
            .Select(s => s.Id)
            .ToListAsync(cancellationToken);

        if (sessionIds.Count == 0)
        {
            return;
        }

        var messages = await _dbContext.ChatMessages
            .Where(m => sessionIds.Contains(m.SessionId))
            .ToListAsync(cancellationToken);

        _dbContext.ChatMessages.RemoveRange(messages);

        var sessions = await _dbContext.ChatSessions
            .Where(s => sessionIds.Contains(s.Id))
            .ToListAsync(cancellationToken);

        _dbContext.ChatSessions.RemoveRange(sessions);
    }

    public async Task DeleteAllByResourceIdAsync(Guid resourceId, Guid userId, CancellationToken cancellationToken)
    {
        var sessionIds = await _dbContext.ChatSessions
            .Where(s => s.ResourceId == resourceId && s.UserId == userId)
            .Select(s => s.Id)
            .ToListAsync(cancellationToken);

        if (sessionIds.Count == 0)
        {
            return;
        }

        var messages = await _dbContext.ChatMessages
            .Where(m => sessionIds.Contains(m.SessionId))
            .ToListAsync(cancellationToken);

        _dbContext.ChatMessages.RemoveRange(messages);

        var sessions = await _dbContext.ChatSessions
            .Where(s => sessionIds.Contains(s.Id))
            .ToListAsync(cancellationToken);

        _dbContext.ChatSessions.RemoveRange(sessions);
    }

    public Task AddMessagesAsync(IEnumerable<ChatMessage> messages, CancellationToken cancellationToken)
    {
        _dbContext.ChatMessages.AddRange(messages);
        return Task.CompletedTask;
    }

    public async Task<IReadOnlyList<ChatMessage>> ListMessagesByChatSessionIdAsync(Guid chatSessionId, CancellationToken cancellationToken)
    {
        return await _dbContext.ChatMessages
            .AsNoTracking()
            .Where(m => m.SessionId == chatSessionId)
            .OrderBy(m => m.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ChatMessage>> ListUserVisibleMessagesByChatSessionIdAsync(Guid chatSessionId, CancellationToken cancellationToken)
    {
        return await _dbContext.ChatMessages
            .AsNoTracking()
            .Where(m => m.SessionId == chatSessionId && (m.Role == ChatRole.User || m.Role == ChatRole.Assistant))
            .OrderBy(m => m.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<(IReadOnlyList<ChatSession> ChatSessions, int TotalCount)> GetPageByMentorIdAsync(Guid mentorId, Guid userId, int pageNumber, int pageSize, CancellationToken cancellationToken)
    {
        var query = _dbContext.ChatSessions
            .AsNoTracking()
            .Where(s => s.MentorId == mentorId && s.UserId == userId)
            .OrderByDescending(s => s.UpdatedAt);

        var totalCount = await query.CountAsync(cancellationToken);
        var sessions = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (sessions, totalCount);
    }

    public async Task<(IReadOnlyList<ChatSession> ChatSessions, int TotalCount)> GetPageByResourceIdAsync(Guid resourceId, Guid userId, int pageNumber, int pageSize, CancellationToken cancellationToken)
    {
        var query = _dbContext.ChatSessions
            .AsNoTracking()
            .Where(s => s.ResourceId == resourceId && s.UserId == userId)
            .OrderByDescending(s => s.UpdatedAt);

        var totalCount = await query.CountAsync(cancellationToken);
        var sessions = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (sessions, totalCount);
    }

    public async Task<(IReadOnlyList<ChatSession> ChatSessions, int TotalCount)> GetPageGeneralByUserIdAsync(Guid userId, int pageNumber, int pageSize, CancellationToken cancellationToken)
    {
        var query = _dbContext.ChatSessions
            .AsNoTracking()
            .Where(s => s.UserId == userId && s.MentorId == null && s.ResourceId == null)
            .OrderByDescending(s => s.UpdatedAt);

        var totalCount = await query.CountAsync(cancellationToken);
        var sessions = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (sessions, totalCount);
    }

}
