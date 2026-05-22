namespace Aethria.Domain.Entities;

public class QuizVersion : BaseEntity
{
    public Guid QuizId { get; set; }
    public int VersionNumber { get; set; }
    public ICollection<QuestionSnapshot> QuestionSnapshots { get; set; } = [];
}
