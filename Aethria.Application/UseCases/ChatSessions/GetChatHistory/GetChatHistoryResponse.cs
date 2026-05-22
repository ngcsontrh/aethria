namespace Aethria.Application.UseCases.ChatSessions.GetChatHistory;

public sealed record ChatMessageResponse(
    Guid Id,
    string Role,
    string Content,
    DateTimeOffset CreatedAt);

public sealed record GetChatHistoryResponse(
    IReadOnlyList<ChatMessageResponse> Messages);
