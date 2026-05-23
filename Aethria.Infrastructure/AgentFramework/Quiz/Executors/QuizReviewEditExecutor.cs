using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using System.ComponentModel;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Aethria.Infrastructure.AgentFramework.Quiz.Executors;

[SendsMessage(typeof(QuizReviewEditOutput))]
internal partial class QuizReviewEditExecutor : Executor
{
    private const int RelevantChunkCount = 5;

    private readonly IChatClient _reviewerChatClient;
    private readonly IChatClient _editorChatClient;
    private readonly IResourceChunkVectorStore _resourceChunkVectorStore;
    private readonly bool _enableSensitiveTelemetry;

    public QuizReviewEditExecutor(
        IChatClient reviewerChatClient,
        IChatClient editorChatClient,
        IResourceChunkVectorStore resourceChunkVectorStore,
        bool enableSensitiveTelemetry) : base("QuizReviewEditExecutor")
    {
        _reviewerChatClient = reviewerChatClient;
        _editorChatClient = editorChatClient;
        _resourceChunkVectorStore = resourceChunkVectorStore;
        _enableSensitiveTelemetry = enableSensitiveTelemetry;
    }

    [MessageHandler]
    public async ValueTask<QuizReviewEditOutput> HandleAsync(
        QuizGeneratorOutput message,
        IWorkflowContext context,
        CancellationToken cancellationToken)
    {
        var assignments = await CreateReviewAssignmentsAsync(message, cancellationToken);
        var results = await RunReviewEditAsync(assignments, cancellationToken);
        var failures = results
            .Where(r => !r.IsSuccess)
            .ToList();

        if (failures.Count > 0)
        {
            return new QuizReviewEditOutput
            {
                IsSuccess = false,
                ErrorMessage = $"Quiz review/edit failed for question(s): {string.Join(" | ", failures.Select(f => $"Q{f.QuestionNumber}: {f.ErrorMessage}"))}"
            };
        }

        return new QuizReviewEditOutput
        {
            IsSuccess = true,
            Questions = results
                .OrderBy(r => r.QuestionNumber)
                .Select(r => r.Question!)
                .ToList()
        };
    }

    private async Task<List<QuizQuestionAssignment>> CreateReviewAssignmentsAsync(
        QuizGeneratorOutput message,
        CancellationToken cancellationToken)
    {
        var assignments = new List<QuizQuestionAssignment>(message.Questions.Count);

        for (var i = 0; i < message.Questions.Count; i++)
        {
            var chunks = await _resourceChunkVectorStore.GetRelevantChunksAsync(
                message.ResourceId,
                message.Questions[i].QuestionText,
                RelevantChunkCount,
                cancellationToken);

            assignments.Add(new QuizQuestionAssignment
            {
                QuestionNumber = i + 1,
                Question = message.Questions[i],
                Chunks = chunks
                    .OrderBy(c => c.ChunkIndex)
                    .Select(c => new QuizReviewChunk
                    {
                        ChunkIndex = c.ChunkIndex,
                        Content = c.Content
                    })
                    .ToList()
            });
        }

        return assignments;
    }

    private async Task<List<QuizQuestionProcessResult>> RunReviewEditAsync(
        IReadOnlyList<QuizQuestionAssignment> assignments,
        CancellationToken cancellationToken)
    {
        //var results = new List<QuizQuestionProcessResult>(assignments.Count);

        //foreach (var assignment in assignments)
        //{
        //    cancellationToken.ThrowIfCancellationRequested();
        //    results.Add(await ReviewEditQuestionAsync(assignment, cancellationToken));
        //}

        //return results;

        var tasks = assignments.Select(a => ReviewEditQuestionAsync(a, cancellationToken));
        var results = await Task.WhenAll(tasks);
        return results.ToList();
    }

    private async Task<QuizQuestionProcessResult> ReviewEditQuestionAsync(
        QuizQuestionAssignment assignment,
        CancellationToken cancellationToken)
    {
        var review = await RunStructuredReviewAsync(assignment, cancellationToken);

        if (!string.Equals(review.Outcome, "needs_revision", StringComparison.OrdinalIgnoreCase))
        {
            return QuizQuestionProcessResult.Success(
                assignment.QuestionNumber,
                assignment.Question);
        }

        return await RunStructuredEditAsync(assignment, review, cancellationToken);
    }

    private async Task<QuizReviewAIResponse> RunStructuredReviewAsync(
        QuizQuestionAssignment assignment,
        CancellationToken cancellationToken)
    {
        var agent = _reviewerChatClient
            .AsAIAgent(
                name: $"QuizRagReviewerQ{assignment.QuestionNumber}",
                instructions: QuizInstructions.ReviewerInstruction)
            .AsBuilder()
            .UseOpenTelemetry(configure: telemetry => telemetry.EnableSensitiveData = _enableSensitiveTelemetry)
            .Build();

        try
        {
            var response = await agent.RunAsync<QuizReviewAIResponse>(
                BuildReviewerPrompt(assignment),
                options: new ChatClientAgentRunOptions(new ChatOptions
                {
                    Temperature = 0.0f,
                    TopP = 1.0f
                }),
                cancellationToken: cancellationToken);

            if (response.Result is null)
            {
                return new QuizReviewAIResponse
                {
                    QuestionNumber = assignment.QuestionNumber,
                    Outcome = "needs_revision",
                    Feedback = "RAG reviewer returned an empty structured response."
                };
            }

            var review = response.Result;

            if (review.QuestionNumber <= 0)
            {
                review.QuestionNumber = assignment.QuestionNumber;
            }

            review.Outcome = string.Equals(review.Outcome, "needs_revision", StringComparison.OrdinalIgnoreCase)
                ? "needs_revision"
                : "approved";
            review.Feedback = review.Outcome == "needs_revision" ? review.Feedback : string.Empty;

            return review;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            return new QuizReviewAIResponse
            {
                QuestionNumber = assignment.QuestionNumber,
                Outcome = "needs_revision",
                Feedback = $"RAG reviewer structured output failed: {ex.Message}"
            };
        }
    }

    private async Task<QuizQuestionProcessResult> RunStructuredEditAsync(
        QuizQuestionAssignment assignment,
        QuizReviewAIResponse review,
        CancellationToken cancellationToken)
    {
        var agent = _editorChatClient
            .AsAIAgent(
                name: $"QuizEditorQ{assignment.QuestionNumber}",
                instructions: QuizInstructions.EditorInstruction)
            .AsBuilder()
            .UseOpenTelemetry(configure: telemetry => telemetry.EnableSensitiveData = _enableSensitiveTelemetry)
            .Build();

        try
        {
            var response = await agent.RunAsync<QuizEditedQuestionAIResponse>(
                BuildEditorPrompt(assignment, review),
                options: new ChatClientAgentRunOptions(new ChatOptions
                {
                    Temperature = 0.1f,
                    TopP = 0.9f
                }),
                cancellationToken: cancellationToken);

            if (response.Result is null)
            {
                return QuizQuestionProcessResult.Failed(
                    assignment.QuestionNumber,
                    "Structured edit returned an empty response.");
            }

            var editedQuestion = response.Result;

            if (editedQuestion.QuestionNumber <= 0)
            {
                editedQuestion.QuestionNumber = assignment.QuestionNumber;
            }

            if (editedQuestion.QuestionNumber != assignment.QuestionNumber)
            {
                return QuizQuestionProcessResult.Failed(
                    assignment.QuestionNumber,
                    $"Editor returned questionNumber {editedQuestion.QuestionNumber}.");
            }

            if (!TryCreateCandidate(editedQuestion, out var candidate, out var errorMessage))
            {
                return QuizQuestionProcessResult.Failed(
                    assignment.QuestionNumber,
                    $"Editor returned invalid output: {errorMessage}");
            }

            return QuizQuestionProcessResult.Success(
                assignment.QuestionNumber,
                candidate);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            return QuizQuestionProcessResult.Failed(
                assignment.QuestionNumber,
                $"Structured edit failed: {ex.Message}");
        }
    }

    private static string BuildReviewerPrompt(QuizQuestionAssignment assignment)
    {
        var sb = new StringBuilder();
        sb.AppendLine("## Workflow Step");
        sb.AppendLine("Review this single generated quiz question for QuizAgentWorkflow.");
        sb.AppendLine();
        sb.AppendLine("## Task");
        sb.AppendLine("Decide whether the assigned question is approved or needs_revision. Do not edit the question.");
        sb.AppendLine();
        sb.AppendLine("## Assigned Question JSON");
        sb.AppendLine(JsonSerializer.Serialize(new
        {
            assignment.QuestionNumber,
            assignment.Question.QuestionText,
            assignment.Question.Options,
            assignment.Question.CorrectOptionIndex,
            assignment.Question.Explanation
        }));
        sb.AppendLine();
        sb.AppendLine("## Retrieved Resource Chunks");

        if (assignment.Chunks.Count == 0)
        {
            sb.AppendLine("No relevant chunks were retrieved. Mark the question as needs_revision because grounding cannot be verified.");
        }
        else
        {
            foreach (var chunk in assignment.Chunks)
            {
                sb.AppendLine($"[Chunk {chunk.ChunkIndex}]");
                sb.AppendLine(chunk.Content);
                sb.AppendLine();
            }
        }

        return sb.ToString();
    }

    private static string BuildEditorPrompt(
        QuizQuestionAssignment assignment,
        QuizReviewAIResponse review)
    {
        var sb = new StringBuilder();
        sb.AppendLine("## Workflow Step");
        sb.AppendLine("Edit one rejected quiz question for QuizAgentWorkflow.");
        sb.AppendLine();
        sb.AppendLine("## Task");
        sb.AppendLine("Return the final replacement for this question only. Approved questions are handled elsewhere and must not be mentioned.");
        sb.AppendLine();
        sb.AppendLine("## Reviewer Feedback");
        sb.AppendLine(review.Feedback);
        sb.AppendLine();
        sb.AppendLine("## Rejected Question JSON");
        sb.AppendLine(JsonSerializer.Serialize(new
        {
            assignment.QuestionNumber,
            CurrentQuestion = assignment.Question
        }));
        sb.AppendLine();
        sb.AppendLine("## Retrieved Resource Chunks");

        if (assignment.Chunks.Count == 0)
        {
            sb.AppendLine("No relevant chunks were retrieved. Rewrite only when the reviewer feedback can be addressed without adding unsupported facts.");
        }
        else
        {
            foreach (var chunk in assignment.Chunks)
            {
                sb.AppendLine($"[Chunk {chunk.ChunkIndex}]");
                sb.AppendLine(chunk.Content);
                sb.AppendLine();
            }
        }

        return sb.ToString();
    }

    private static bool TryCreateCandidate(
        QuizEditedQuestionAIResponse editedQuestion,
        out QuizQuestionCandidate candidate,
        out string errorMessage)
    {
        candidate = new QuizQuestionCandidate();
        errorMessage = string.Empty;

        if (string.IsNullOrWhiteSpace(editedQuestion.QuestionText))
        {
            errorMessage = "questionText is required.";
            return false;
        }

        if (editedQuestion.Options.Count != 4)
        {
            errorMessage = "options must contain exactly 4 items.";
            return false;
        }

        if (editedQuestion.Options.Any(string.IsNullOrWhiteSpace))
        {
            errorMessage = "options cannot contain empty items.";
            return false;
        }

        if (editedQuestion.CorrectOptionIndex is < 0 or > 3)
        {
            errorMessage = "correctOptionIndex must be between 0 and 3.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(editedQuestion.Explanation))
        {
            errorMessage = "explanation is required.";
            return false;
        }

        candidate = new QuizQuestionCandidate
        {
            QuestionText = editedQuestion.QuestionText,
            Options = editedQuestion.Options,
            CorrectOptionIndex = editedQuestion.CorrectOptionIndex,
            Explanation = editedQuestion.Explanation
        };

        return true;
    }

    private sealed class QuizQuestionAssignment
    {
        public int QuestionNumber { get; init; }
        public QuizQuestionCandidate Question { get; init; } = null!;
        public List<QuizReviewChunk> Chunks { get; init; } = [];
    }

    private sealed class QuizReviewChunk
    {
        public int ChunkIndex { get; init; }
        public string Content { get; init; } = null!;
    }

    private sealed class QuizReviewAIResponse
    {
        [JsonPropertyName("questionNumber")]
        [Description("The one-based question number being reviewed.")]
        public int QuestionNumber { get; set; }

        [JsonPropertyName("outcome")]
        [Description("'approved' or 'needs_revision'.")]
        public string Outcome { get; set; } = null!;

        [JsonPropertyName("feedback")]
        [Description("Feedback explaining issues found. Empty if approved.")]
        public string Feedback { get; set; } = null!;
    }

    private sealed class QuizEditedQuestionAIResponse
    {
        [JsonPropertyName("questionNumber")]
        [Description("The one-based question number that this edited question replaces.")]
        public int QuestionNumber { get; set; }

        [JsonPropertyName("questionText")]
        [Description("The revised quiz question text.")]
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

    private sealed class QuizQuestionProcessResult
    {
        public int QuestionNumber { get; init; }
        public QuizQuestionCandidate? Question { get; init; }
        public string? ErrorMessage { get; init; }
        public bool IsSuccess { get; init; }

        public static QuizQuestionProcessResult Success(
            int questionNumber,
            QuizQuestionCandidate question)
        {
            return new QuizQuestionProcessResult
            {
                IsSuccess = true,
                QuestionNumber = questionNumber,
                Question = question
            };
        }

        public static QuizQuestionProcessResult Failed(
            int questionNumber,
            string errorMessage)
        {
            return new QuizQuestionProcessResult
            {
                IsSuccess = false,
                QuestionNumber = questionNumber,
                ErrorMessage = errorMessage
            };
        }
    }
}
