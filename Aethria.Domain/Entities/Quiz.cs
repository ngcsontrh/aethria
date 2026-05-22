namespace Aethria.Domain.Entities;

public class Quiz : AggregateRoot
{
    public Guid UserId { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public Guid? ResourceId { get; set; }
    public int CurrentVersionNumber { get; set; }

    public ICollection<QuizQuestion> Questions { get; set; } = [];
    public ICollection<QuizVersion> Versions { get; set; } = [];
}
