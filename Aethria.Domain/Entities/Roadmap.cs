namespace Aethria.Domain.Entities;

public class Roadmap : AggregateRoot
{
    public Guid UserId { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public string Content { get; set; } = null!;
    public string? Mermaid { get; set; }
    public Guid ResourceId { get; set; }
}
