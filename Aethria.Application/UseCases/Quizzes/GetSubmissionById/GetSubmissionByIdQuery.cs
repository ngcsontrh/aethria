namespace Aethria.Application.UseCases.Quizzes.GetSubmissionById;

public sealed record GetSubmissionByIdQuery(
    Guid QuizId,
    Guid SubmissionId,
    Guid UserId) : IRequest<GetSubmissionByIdQuery, ValueTask<Result<GetSubmissionByIdResponse>>>;
