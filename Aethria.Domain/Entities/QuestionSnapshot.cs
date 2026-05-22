namespace Aethria.Domain.Entities;

public class QuestionSnapshot : BaseEntity
{
    public Guid QuizVersionId { get; set; }
    public Guid OriginalQuestionId { get; set; }
    public string Text { get; set; } = null!;
    public string Explanation { get; set; } = null!;
    public Guid CorrectOptionId { get; set; }
    public int OrderIndex { get; set; }
    public ICollection<QuestionOptionSnapshot> Options { get; set; } = [];
}
