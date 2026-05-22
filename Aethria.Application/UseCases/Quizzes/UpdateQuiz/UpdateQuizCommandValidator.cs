namespace Aethria.Application.UseCases.Quizzes.UpdateQuiz;

internal sealed class UpdateQuizCommandValidator : AbstractValidator<UpdateQuizCommand>
{
    public UpdateQuizCommandValidator()
    {
        RuleFor(command => command.QuizId)
            .NotEmpty()
            .WithMessage("QuizId is required.");

        RuleFor(command => command.UserId)
            .NotEmpty()
            .WithMessage("UserId is required.");

        RuleFor(command => command.Questions)
            .Must(questions => questions is null || questions.Count > 0)
            .WithMessage("Quiz must have at least one question.");

        RuleForEach(command => command.Questions)
            .ChildRules(question =>
            {
                question.RuleFor(item => item.Text)
                    .Must(text => !string.IsNullOrWhiteSpace(text))
                    .WithMessage("Question text is required.");

                question.RuleFor(item => item.Explanation)
                    .Must(explanation => !string.IsNullOrWhiteSpace(explanation))
                    .WithMessage("Question explanation is required.");

                question.RuleFor(item => item.Options)
                    .Must(options => options is not null && options.Count >= 2)
                    .WithMessage("Each question must have at least 2 options.");

                question.RuleForEach(item => item.Options)
                    .ChildRules(option =>
                    {
                        option.RuleFor(item => item.Text)
                            .Must(text => !string.IsNullOrWhiteSpace(text))
                            .WithMessage("Each option must contain text.");
                    });

                question.RuleFor(item => item.CorrectOptionIndex)
                    .Must((item, correctOptionIndex) =>
                        item.Options is not null &&
                        correctOptionIndex >= 0 &&
                        correctOptionIndex < item.Options.Count)
                    .WithMessage(item =>
                        $"CorrectOptionIndex {item.CorrectOptionIndex} is out of range for question with {item.Options?.Count ?? 0} options.");
            });
    }
}
