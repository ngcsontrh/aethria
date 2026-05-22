namespace Aethria.Domain.Entities;

public class QuizSubmission : AggregateRoot
{
    public Guid UserId { get; set; }
    public Guid QuizId { get; set; }
    public Guid QuizVersionId { get; set; }
    public int Score { get; set; }
    public int TotalQuestions { get; set; }
    public bool IsPassed { get; set; }
    public ICollection<SubmissionAnswer> Answers { get; set; } = [];
}
