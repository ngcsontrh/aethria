namespace Aethria.Domain.Entities;

public class QuestionOption : BaseEntity
{
    public Guid QuizQuestionId { get; set; }
    public string Text { get; set; } = null!;
    public int OrderIndex { get; set; }
}
