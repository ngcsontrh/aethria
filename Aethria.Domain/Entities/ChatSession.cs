namespace Aethria.Domain.Entities;

public class ChatSession : AggregateRoot
{
    public Guid UserId { get; set; }
    public Guid? MentorId { get; set; }
    public Guid? ResourceId { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }

    public ICollection<ChatMessage> Messages { get; set; } = [];
}
