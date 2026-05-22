namespace Aethria.Domain.Entities;

public class QuizQuestion : BaseEntity
{
    public Guid QuizId { get; set; }
    public string Text { get; set; } = null!;
    public ICollection<QuestionOption> Options { get; set; } = [];
    public Guid CorrectOptionId { get; set; }
    public string Explanation { get; set; } = null!;
    public int OrderIndex { get; set; }
}
