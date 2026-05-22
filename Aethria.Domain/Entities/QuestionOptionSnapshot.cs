namespace Aethria.Domain.Entities;

public class QuestionOptionSnapshot : BaseEntity
{
    public Guid QuestionSnapshotId { get; set; }
    public Guid OriginalOptionId { get; set; }
    public string Text { get; set; } = null!;
    public int OrderIndex { get; set; }
}
