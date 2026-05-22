using Microsoft.Agents.AI.Workflows;

namespace Aethria.Infrastructure.AgentFramework.Quiz.Executors;

[YieldsOutput(typeof(QuizGenerationWorkflowResult))]
internal partial class QuizFinalizerExecutor : Executor
{
    public QuizFinalizerExecutor() : base("QuizFinalizerExecutor")
    {
    }

    [MessageHandler]
    public async ValueTask HandleAsync(
        QuizReviewEditOutput message,
        IWorkflowContext context,
        CancellationToken cancellationToken)
    {
        if (!message.IsSuccess)
        {
            await context.YieldOutputAsync(new QuizGenerationWorkflowResult
            {
                IsSuccess = false,
                ErrorMessage = message.ErrorMessage ?? "Quiz editor returned invalid output."
            }, cancellationToken);
            return;
        }

        await context.YieldOutputAsync(new QuizGenerationWorkflowResult
        {
            IsSuccess = true,
            Questions = ToGeneratedQuestions(message.Questions)
        }, cancellationToken);
    }

    private static List<GeneratedQuestion> ToGeneratedQuestions(IReadOnlyList<QuizQuestionCandidate> candidates)
    {
        var questions = new List<GeneratedQuestion>(candidates.Count);

        for (var i = 0; i < candidates.Count; i++)
        {
            var candidate = candidates[i];
            questions.Add(new GeneratedQuestion
            {
                QuestionNumber = i + 1,
                QuestionText = candidate.QuestionText,
                Options = candidate.Options,
                CorrectOptionIndex = candidate.CorrectOptionIndex,
                Explanation = candidate.Explanation
            });
        }

        return questions;
    }
}
