using Aethria.Application.UseCases.Chat.Contracts;
using Azure;
using Azure.AI.OpenAI;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Compaction;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Hosting;
using System.Runtime.CompilerServices;
using Tavily;

#pragma warning disable MAAI001

namespace Aethria.Infrastructure.AgentFramework.Chat;

internal sealed class MentorChatAgent : IChatAgent
{
    private readonly FoundryOptions _options;
    private readonly TavilyClient _tavilyClient;
    private readonly bool _enableSensitiveTelemetry;

    public MentorChatAgent(
        IOptions<FoundryOptions> options,
        TavilyClient tavilyClient,
        IHostEnvironment hostEnvironment)
    {
        _options = options.Value;
        _tavilyClient = tavilyClient;
        _enableSensitiveTelemetry = hostEnvironment.IsDevelopment();
    }

    public async IAsyncEnumerable<ChatAgentStreamResult> RunStreamingAsync(
        ChatAgentContext context,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var agent = CreateAgent(context.Instruction ?? string.Empty, context.Tools ?? []);
        var chatMessages = AgentChatMessageMapper.ToChatMessages(context.Messages);
        var updates = new List<AgentResponseUpdate>();

        await foreach (var update in agent.RunStreamingAsync(chatMessages, cancellationToken: cancellationToken))
        {
            updates.Add(update);

            if (!string.IsNullOrEmpty(update.Text))
            {
                yield return new ChatAgentStreamResult(Delta: update.Text);
            }
        }

        var response = updates.ToAgentResponse();
        yield return new ChatAgentStreamResult(
            Answer: response.Text,
            Messages: AgentChatMessageMapper.ToStreamMessages(response.Messages),
            IsCompleted: true);
    }

    private AIAgent CreateAgent(string instruction, IReadOnlyList<string> tools)
    {
        var aiTools = CreateTools(tools);
        var azureOpenAIClient = new AzureOpenAIClient(
            new Uri(_options.AzureOpenAIEndPoint),
            new AzureKeyCredential(_options.ApiKey));
        var builder = azureOpenAIClient.GetChatClient("gpt-5.4-mini").AsIChatClient().AsBuilder();
        builder.UseFunctionInvocation();
        builder.UseAIContextProviders(new CompactionProvider(CreateCompactionPipeline()));

        var agent = builder.Build().AsAIAgent(
            name: "CustomMentorAgent",
            instructions: instruction.Trim(),
            tools: aiTools);

        return agent.AsBuilder()
            .UseOpenTelemetry(configure: telemetry => telemetry.EnableSensitiveData = _enableSensitiveTelemetry)
            .Build();
    }

    private List<AITool> CreateTools(IReadOnlyList<string> tools)
    {
        var aiTools = new List<AITool>();

        foreach (var tool in tools)
        {
            var validTool = AvailableAgentTools.Tools.FirstOrDefault(t =>
                t.Id.Equals(tool, StringComparison.OrdinalIgnoreCase));

            if (validTool?.Id == AvailableAgentTools.WebSearchId)
            {
                aiTools.Add(_tavilyClient.AsSearchTool());
            }
            else if (validTool?.Id == AvailableAgentTools.WebExtractId)
            {
                aiTools.Add(_tavilyClient.AsExtractTool());
            }
        }

        return aiTools;
    }

    private PipelineCompactionStrategy CreateCompactionPipeline()
    {
        var azureOpenAIClient = new AzureOpenAIClient(
            new Uri(_options.AzureOpenAIEndPoint),
            new AzureKeyCredential(_options.ApiKey));
        var summarizerClient = azureOpenAIClient.GetChatClient("gpt-4.1-nano").AsIChatClient();

        return new PipelineCompactionStrategy(
            new ToolResultCompactionStrategy(CompactionTriggers.TokensExceed(64_000)),
            new SummarizationCompactionStrategy(summarizerClient, CompactionTriggers.TokensExceed(256_000)),
            new TruncationCompactionStrategy(CompactionTriggers.TokensExceed(384_000)));
    }

}
