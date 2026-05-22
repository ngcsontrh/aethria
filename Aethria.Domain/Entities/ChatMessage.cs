namespace Aethria.Domain.Entities;

public class ChatMessage : BaseEntity
{
    public Guid SessionId { get; set; }
    public ChatRole Role { get; set; } = null!;
    public string Content { get; set; } = null!;
}
