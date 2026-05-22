using System.ComponentModel;
using System.Text;
using System.Text.Json.Serialization;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;

namespace Aethria.Infrastructure.AgentFramework.Quiz.Executors;

[SendsMessage(typeof(QuizGeneratorOutput))]
internal partial class QuizGeneratorExecutor : Executor
{
    private readonly AIAgent _generatorAgent;

    public QuizGeneratorExecutor(AIAgent generatorAgent) : base("QuizGeneratorExecutor")
    {
        _generatorAgent = generatorAgent;
    }

    [MessageHandler]
    public async ValueTask<QuizGeneratorOutput> HandleAsync(
        QuizGeneratorInput message,
        IWorkflowContext context,
        CancellationToken cancellationToken)
    {
        var sb = new StringBuilder();
        sb.AppendLine("## Workflow Step");
        sb.AppendLine("Generate the initial quiz question batch for QuizAgentWorkflow.");
        sb.AppendLine();
        sb.AppendLine("## Task");
        sb.AppendLine();
        sb.AppendLine($"Generate exactly {message.NumberOfQuestions} quiz questions from the source content below.");
        sb.AppendLine("The next workflow step will review and edit individual questions, but this generated batch must already be clear, grounded, and structurally valid.");
        sb.AppendLine();
        sb.AppendLine("## Source Content");
        sb.AppendLine();
        sb.AppendLine(message.SourceContent);

        if (!string.IsNullOrWhiteSpace(message.UserPrompt))
        {
            sb.AppendLine();
            sb.AppendLine("## Additional User Instructions");
            sb.AppendLine();
            sb.AppendLine("Apply these instructions only when they are compatible with the quiz generation rules and structured output schema.");
            sb.AppendLine(message.UserPrompt);
        }

        var response = await _generatorAgent.RunAsync<QuizGenerateResponse>(
            sb.ToString(),
            options: new ChatClientAgentRunOptions(new ChatOptions
            {
                Temperature = 0.3f,
                TopP = 0.9f
            }),
            cancellationToken: cancellationToken);

        return new QuizGeneratorOutput
        {
            ResourceId = message.ResourceId,
            Questions = response.Result.Questions.Select(q => new QuizQuestionCandidate
            {
                QuestionText = q.QuestionText,
                Options = q.Options,
                CorrectOptionIndex = q.CorrectOptionIndex,
                Explanation = q.Explanation
            }).ToList()
        };
    }

    private sealed class QuizGenerateResponse
    {
        [JsonPropertyName("questions")]
        [Description("List of generated quiz questions.")]
        public List<QuizQuestionAIResponse> Questions { get; set; } = [];
    }

    private sealed class QuizQuestionAIResponse
    {
        [JsonPropertyName("questionText")]
        [Description("The quiz question text.")]
        public string QuestionText { get; set; } = null!;

        [JsonPropertyName("options")]
        [Description("Array of 4 multiple-choice options.")]
        public List<string> Options { get; set; } = [];

        [JsonPropertyName("correctOptionIndex")]
        [Description("Zero-based index of the correct option.")]
        public int CorrectOptionIndex { get; set; }

        [JsonPropertyName("explanation")]
        [Description("Explanation of why the correct answer is right.")]
        public string Explanation { get; set; } = null!;
    }
}
