using Aethria.Application.Abstractions.Embedding;
using Azure;
using Azure.AI.OpenAI;
using OpenAI.Embeddings;

namespace Aethria.Infrastructure.Embedding;

internal sealed class AzureOpenAIEmbeddingService : IEmbeddingService
{
    private const string DeploymentName = "text-embedding-3-small";

    private const int BatchSize = 50;

    private readonly EmbeddingClient _embeddingClient;

    public AzureOpenAIEmbeddingService(
        IOptions<FoundryOptions> options)
    {
        var client = new AzureOpenAIClient(
            new Uri(options.Value.AzureOpenAIEndPoint),
            new AzureKeyCredential(options.Value.ApiKey));

        _embeddingClient = client.GetEmbeddingClient(DeploymentName);
    }

    public async Task<ReadOnlyMemory<float>> GenerateEmbeddingAsync(
        string text,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return ReadOnlyMemory<float>.Empty;
        }

        var response = await _embeddingClient.GenerateEmbeddingAsync(
            text,
            cancellationToken: cancellationToken);

        return response.Value.ToFloats();
    }

    public async Task<IList<ReadOnlyMemory<float>>> GenerateEmbeddingsAsync(
        IList<string> texts,
        CancellationToken cancellationToken)
    {
        if (texts.Count == 0)
        {
            return [];
        }

        var results = new ReadOnlyMemory<float>[texts.Count];
        var indexedTexts = texts.Select((text, index) => (Text: text, Index: index));

        foreach (var batch in indexedTexts.Chunk(BatchSize))
        {
            cancellationToken.ThrowIfCancellationRequested();

            var validItems = batch
                .Where(static x => !string.IsNullOrWhiteSpace(x.Text))
                .ToArray();

            if (validItems.Length == 0)
            {
                continue;
            }

            var response = await _embeddingClient.GenerateEmbeddingsAsync(
                validItems.Select(static x => x.Text).ToArray(),
                cancellationToken: cancellationToken);

            var embeddings = response.Value.Select(static x => x.ToFloats()).ToArray();

            for (var i = 0; i < validItems.Length; i++)
            {
                results[validItems[i].Index] = embeddings[i];
            }
        }

        return results;
    }
}
