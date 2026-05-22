namespace Aethria.Application.UseCases.Quizzes.GetQuizQuestions;

public sealed record QuizQuestionResponse(
    Guid Id,
    string Text,
    int OrderIndex,
    IReadOnlyList<QuestionOptionResponse> Options);

public sealed record QuestionOptionResponse(Guid Id, string Text, int OrderIndex);

public sealed record GetQuizQuestionsResponse(Guid? QuizVersionId, IReadOnlyList<QuizQuestionResponse> Questions);
