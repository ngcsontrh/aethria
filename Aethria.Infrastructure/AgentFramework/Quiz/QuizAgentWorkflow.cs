using Aethria.Infrastructure.AgentFramework.Quiz.Executors;
using Azure;
using Azure.AI.OpenAI;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Hosting;
using System.Runtime.CompilerServices;

namespace Aethria.Infrastructure.AgentFramework.Quiz;

internal sealed class QuizAgentWorkflow : IAIQuizGenerationWorkflow
{
    private readonly FoundryOptions _foundryOptions;
    private readonly IResourceChunkVectorStore _resourceChunkVectorStore;
    private readonly bool _enableSensitiveTelemetry;

    public QuizAgentWorkflow(
        IOptions<FoundryOptions> options,
        IResourceChunkVectorStore resourceChunkVectorStore,
        IHostEnvironment hostEnvironment)
    {
        _foundryOptions = options.Value;
        _resourceChunkVectorStore = resourceChunkVectorStore;
        _enableSensitiveTelemetry = hostEnvironment.IsDevelopment();
    }

    public async IAsyncEnumerable<CreateAIQuizResult> RunAsync(
        CreateAIQuizInput input,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(input.SourceContent))
        {
            yield return CreateAIQuizResult.Failed("Source content is empty or null.");
            yield break;
        }

        yield return CreateAIQuizResult.Progress(CreateAIQuizResult.Statuses.Started, "Starting quiz generation workflow.");

        var azureOpenAIClient = new AzureOpenAIClient(
            new Uri(_foundryOptions.AzureOpenAIEndPoint),
            new AzureKeyCredential(_foundryOptions.ApiKey));

        var generatorAgent = azureOpenAIClient.GetChatClient("gpt-5.4").AsIChatClient()
            .AsAIAgent(
                name: "QuizGeneratorAgent",
                instructions: QuizInstructions.GeneratorInstruction)
            .AsBuilder()
            .UseOpenTelemetry(configure: telemetry => telemetry.EnableSensitiveData = _enableSensitiveTelemetry)
            .Build();

        var reviewerChatClient = azureOpenAIClient.GetChatClient("gpt-5.4").AsIChatClient();
        var editorChatClient = azureOpenAIClient.GetChatClient("gpt-5.4-mini").AsIChatClient();

        var generatorExecutor = new QuizGeneratorExecutor(generatorAgent);
        var reviewEditExecutor = new QuizReviewEditExecutor(
            reviewerChatClient,
            editorChatClient,
            _resourceChunkVectorStore,
            _enableSensitiveTelemetry);
        var finalizerExecutor = new QuizFinalizerExecutor();

        var workflow = new WorkflowBuilder(generatorExecutor)
            .AddEdge(generatorExecutor, reviewEditExecutor)
            .AddEdge(reviewEditExecutor, finalizerExecutor)
            .WithOutputFrom(finalizerExecutor)
            .WithOpenTelemetry(configure: telemetry => telemetry.EnableSensitiveData = _enableSensitiveTelemetry)
            .Build();

        yield return CreateAIQuizResult.Progress(CreateAIQuizResult.Statuses.GeneratingQuestions, "Generating quiz questions.");

        var inputMessage = new QuizGeneratorInput
        {
            ResourceId = input.ResourceId,
            SourceContent = input.SourceContent,
            UserPrompt = input.UserPrompt,
            NumberOfQuestions = input.NumberOfQuestions
        };

        var streamingRun = await InProcessExecution.RunStreamingAsync(workflow, inputMessage);
        QuizGenerationWorkflowResult? extractedResult = null;

        await foreach (var evt in streamingRun.WatchStreamAsync().WithCancellation(cancellationToken))
        {
            if (evt is WorkflowOutputEvent outputEvt && outputEvt.Data is QuizGenerationWorkflowResult result)
            {
                extractedResult = result;
                break;
            }
        }

        if (extractedResult is null || !extractedResult.IsSuccess)
        {
            yield return CreateAIQuizResult.Failed(
                extractedResult?.ErrorMessage ?? "  Quiz generation workflow returned no output.");
            yield break;
        }

        yield return CreateAIQuizResult.Completed(
            extractedResult.Questions.Select(q => new AIQuizQuestion
            {
                QuestionNumber = q.QuestionNumber,
                QuestionText = q.QuestionText,
                Options = q.Options,
                CorrectOptionIndex = q.CorrectOptionIndex,
                Explanation = q.Explanation
            }).ToList());
    }
}
