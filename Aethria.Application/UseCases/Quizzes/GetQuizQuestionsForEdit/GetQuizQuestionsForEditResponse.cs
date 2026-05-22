namespace Aethria.Application.UseCases.Quizzes.GetQuizQuestionsForEdit;

public sealed record QuizQuestionForEditResponse(
    Guid Id,
    string Text,
    string Explanation,
    int OrderIndex,
    int CorrectOptionIndex,
    IReadOnlyList<QuestionOptionForEditResponse> Options);

public sealed record QuestionOptionForEditResponse(Guid Id, string Text, int OrderIndex);

public sealed record GetQuizQuestionsForEditResponse(Guid? QuizVersionId, IReadOnlyList<QuizQuestionForEditResponse> Questions);
