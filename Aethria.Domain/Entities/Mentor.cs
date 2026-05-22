namespace Aethria.Domain.Entities;

public class Mentor : AggregateRoot
{
    public Guid UserId { get; set; }
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string Instruction { get; set; } = null!;
    public List<MentorTool> Tools { get; set; } = [];
}
