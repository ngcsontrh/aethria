namespace Aethria.Domain.Entities;

public class SubmissionAnswer : BaseEntity
{
    public Guid QuizSubmissionId { get; set; }
    public Guid QuestionSnapshotId { get; set; }
    public Guid SelectedOptionId { get; set; }
    public bool IsCorrect { get; set; }
}
