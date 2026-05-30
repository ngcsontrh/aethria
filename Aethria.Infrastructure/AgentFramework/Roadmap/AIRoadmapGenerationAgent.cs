using Azure;
using Azure.AI.OpenAI;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Hosting;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace Aethria.Infrastructure.AgentFramework.Roadmap;

internal sealed class AIRoadmapGenerationAgent : IAIRoadmapGenerationAgent
{
    private readonly FoundryOptions _foundryOptions;
    private readonly bool _enableSensitiveTelemetry;

    public AIRoadmapGenerationAgent(
        IOptions<FoundryOptions> options,
        IHostEnvironment hostEnvironment)
    {
        _foundryOptions = options.Value;
        _enableSensitiveTelemetry = hostEnvironment.IsDevelopment();
    }

    public async IAsyncEnumerable<GenerateAIRoadmapResult> RunAsync(
        GenerateAIRoadmapInput input,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(input.SourceContent))
        {
            yield return GenerateAIRoadmapResult.Failed("Source content is empty or null.");
            yield break;
        }

        yield return GenerateAIRoadmapResult.Progress(
            GenerateAIRoadmapResult.Statuses.Started,
            "Starting roadmap generation.");

        yield return GenerateAIRoadmapResult.Progress(
            GenerateAIRoadmapResult.Statuses.GeneratingRoadmap,
            "Generating roadmap.");

        RoadmapGenerateResponse? response = null;
        string? failureMessage = null;
        try
        {
            var azureOpenAIClient = new AzureOpenAIClient(
                new Uri(_foundryOptions.AzureOpenAIEndPoint),
                new AzureKeyCredential(_foundryOptions.ApiKey));

            var agent = azureOpenAIClient.GetChatClient("gpt-5.4-mini").AsIChatClient()
                .AsAIAgent(
                    name: "RoadmapGenerationAgent",
                    instructions: RoadmapGenerationInstructions.SystemPrompt)
                .AsBuilder()
                .UseOpenTelemetry(configure: telemetry => telemetry.EnableSensitiveData = _enableSensitiveTelemetry)
                .Build();

            response = await GenerateRoadmapAsync(agent, input.SourceContent, input.UserPrompt, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception)
        {
            failureMessage = "AI service is temporarily unavailable.";
        }

        if (failureMessage is not null)
        {
            yield return GenerateAIRoadmapResult.Failed(failureMessage);
            yield break;
        }

        var steps = response?.Steps ?? [];
        if (steps.Count == 0)
        {
            yield return GenerateAIRoadmapResult.Failed("AI generated no roadmap steps.");
            yield break;
        }

        var orderedSteps = steps.OrderBy(step => step.StepNumber).ToList();
        var roadmapContent = RoadmapRenderer.RenderMarkdown(orderedSteps);
        var mermaidDiagram = RoadmapRenderer.RenderMermaid(orderedSteps);

        if (string.IsNullOrWhiteSpace(roadmapContent))
        {
            yield return GenerateAIRoadmapResult.Failed("AI generated an empty roadmap.");
            yield break;
        }

        yield return GenerateAIRoadmapResult.Completed(roadmapContent, mermaidDiagram);
    }

    private static async Task<RoadmapGenerateResponse?> GenerateRoadmapAsync(
        AIAgent agent,
        string sourceContent,
        string? userPrompt,
        CancellationToken cancellationToken)
    {
        var response = await agent.RunAsync<RoadmapGenerateResponse>(
            BuildPrompt(sourceContent, userPrompt),
            cancellationToken: cancellationToken);

        try
        {
            return response.Result;
        }
        catch (JsonException)
        {
            if (string.IsNullOrWhiteSpace(response.Text))
            {
                return null;
            }

            var text = response.Text
                .Replace("\r\n", "\\n")
                .Replace("\r", "\\n")
                .Replace("\n", "\\n");

            try
            {
                return JsonSerializer.Deserialize<RoadmapGenerateResponse>(text);
            }
            catch (JsonException)
            {
                return null;
            }
        }
    }

    private static string BuildPrompt(string sourceContent, string? userPrompt)
    {
        var prompt = $"""
            SOURCE_CONTENT:
            {sourceContent}
            """;

        if (!string.IsNullOrWhiteSpace(userPrompt))
        {
            prompt += $"\nAdditional user instructions:\n{userPrompt}";
        }

        return prompt;
    }
}
