namespace Aethria.Application.UseCases.ChatSessions.GetChatSessions;

public sealed record ChatSessionItemResponse(
    Guid Id,
    string Name,
    string? Description,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);
