using Azure;
using Azure.AI.OpenAI;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Compaction;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Hosting;
using System.ComponentModel;
using System.Runtime.CompilerServices;

#pragma warning disable MAAI001

namespace Aethria.Infrastructure.AgentFramework.Chat;

internal sealed class ResourceChatAgent : IChatAgent
{
    private readonly FoundryOptions _options;
    private readonly IResourceChunkVectorStore _resourceChunkVectorStore;
    private readonly bool _enableSensitiveTelemetry;

    public ResourceChatAgent(
        IOptions<FoundryOptions> options,
        IResourceChunkVectorStore resourceChunkVectorStore,
        IHostEnvironment hostEnvironment)
    {
        _options = options.Value;
        _resourceChunkVectorStore = resourceChunkVectorStore;
        _enableSensitiveTelemetry = hostEnvironment.IsDevelopment();
    }

    public async IAsyncEnumerable<ChatAgentStreamResult> RunStreamingAsync(
        ChatAgentContext context,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var resourceId = context.ResourceId ?? throw new InvalidOperationException("ResourceId is required for ResourceChatAgent.");
        var agent = CreateAgent(resourceId);
        var chatMessages = AgentChatMessageMapper.ToChatMessages(context.Messages);
        var updates = new List<AgentResponseUpdate>();

        await foreach (var update in agent.RunStreamingAsync(
                           chatMessages,
                           options: new ChatClientAgentRunOptions(new ChatOptions
                           {
                               Temperature = 0.1f,
                               TopP = 0.9f
                           }),
                           cancellationToken: cancellationToken))
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

    private AIAgent CreateAgent(Guid resourceId)
    {
        var searchTool = CreateSearchResourceChunksTool(resourceId);
        var azureOpenAIClient = new AzureOpenAIClient(
            new Uri(_options.AzureOpenAIEndPoint),
            new AzureKeyCredential(_options.ApiKey));
        var builder = azureOpenAIClient.GetChatClient("gpt-5.4-mini").AsIChatClient().AsBuilder();
        builder.UseFunctionInvocation();
        builder.UseAIContextProviders(new CompactionProvider(CreateCompactionPipeline()));

        var agent = builder.Build().AsAIAgent(
            name: "ResourceRagAgent",
            instructions: ChatAgentInstructions.ResourceRagAgentInstruction,
            tools: [searchTool]);

        return agent.AsBuilder()
            .UseOpenTelemetry(configure: telemetry => telemetry.EnableSensitiveData = _enableSensitiveTelemetry)
            .Build();
    }

    private AITool CreateSearchResourceChunksTool(Guid resourceId)
    {
        return AIFunctionFactory.Create(
            method: async ([Description("The search query to find relevant content in the resource. Formulate a clear, specific query based on what information you need.")] string query, CancellationToken cancellationToken) =>
            {
                var chunks = await _resourceChunkVectorStore.GetRelevantChunksAsync(
                    resourceId, query, topK: 5, cancellationToken);

                if (!chunks.Any())
                {
                    return "No relevant content found in the resource for this query.";
                }

                var sortedChunks = chunks.OrderBy(c => c.ChunkIndex).ToList();

                var formattedChunks = string.Join("\n\n---\n\n", sortedChunks.Select(c => c.Content));

                return formattedChunks;
            },
            name: "search_resource_chunks",
            description: "Search for relevant content chunks from the user's uploaded resource. Use this tool when you need information from the resource to answer the user's question. Do NOT use this tool for greetings, acknowledgments, or follow-up questions that can be answered from conversation history alone.");
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
