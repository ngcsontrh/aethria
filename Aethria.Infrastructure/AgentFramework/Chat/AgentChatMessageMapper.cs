using ChatMessage = Microsoft.Extensions.AI.ChatMessage;
using ChatRole = Microsoft.Extensions.AI.ChatRole;

namespace Aethria.Infrastructure.AgentFramework.Chat;

internal static class AgentChatMessageMapper
{
    public static IReadOnlyList<ChatMessage> ToChatMessages(IEnumerable<(string Role, string Content)> messages)
    {
        return messages
            .Select(message => new ChatMessage(new ChatRole(message.Role), message.Content))
            .ToList();
    }

    public static IReadOnlyList<(string Role, string Content)> ToStreamMessages(IEnumerable<ChatMessage> messages)
    {
        return messages
            .Select(message => (
                Role: message.Role.Value,
                Content: message.Text))
            .ToList();
    }
}
