namespace Aethria.Application.UseCases.Quizzes.GetQuizQuestions;

public sealed record GetQuizQuestionsQuery(Guid QuizId, Guid UserId) : IRequest<GetQuizQuestionsQuery, ValueTask<Result<GetQuizQuestionsResponse>>>;
