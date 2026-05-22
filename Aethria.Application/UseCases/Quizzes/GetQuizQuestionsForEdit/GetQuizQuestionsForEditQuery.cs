namespace Aethria.Application.UseCases.Quizzes.GetQuizQuestionsForEdit;

public sealed record GetQuizQuestionsForEditQuery(Guid QuizId, Guid UserId) : IRequest<GetQuizQuestionsForEditQuery, ValueTask<Result<GetQuizQuestionsForEditResponse>>>;
