using Cohere;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using Qdrant.Client;
using Qdrant.Client.Grpc;
using static Qdrant.Client.Grpc.Conditions;

namespace Aethria.Infrastructure.VectorSearch;

internal sealed class QdrantResourceChunkVectorStore : IResourceChunkVectorStore
{
    private const string CollectionName = "resource_chunks";
    private const string RerankModel = "Cohere-rerank-v4.0-fast-1";
    private const string PayloadResourceId = "resource_id";
    private const string PayloadChunkIndex = "chunk_index";
    private const string PayloadContent = "content";

    private static readonly SemaphoreSlim _collectionSetupLock = new(1, 1);
    private static bool _collectionConfigured;

    private readonly QdrantClient _qdrantClient;
    private readonly IEmbeddingService _embeddingService;
    private readonly CohereClient _cohereClient;
    private readonly ILogger<QdrantResourceChunkVectorStore> _logger;

    public QdrantResourceChunkVectorStore(
        QdrantClient qdrantClient,
        IEmbeddingService embeddingService,
        CohereClient cohereClient,
        ILogger<QdrantResourceChunkVectorStore> logger)
    {
        _qdrantClient = qdrantClient;
        _embeddingService = embeddingService;
        _cohereClient = cohereClient;
        _logger = logger;
    }

    public async Task UpsertAsync(
        Guid resourceId,
        IReadOnlyList<ResourceChunkVectorInput> chunks,
        CancellationToken cancellationToken)
    {
        await EnsureCollectionAsync(cancellationToken);
        await DeleteByResourceIdAsync(resourceId, cancellationToken);

        if (chunks.Count == 0)
        {
            return;
        }

        var points = chunks.Select(chunk => new PointStruct
        {
            Id = chunk.Id,
            Vectors = chunk.Embedding.ToArray(),
            Payload =
            {
                [PayloadResourceId] = resourceId.ToString("D"),
                [PayloadChunkIndex] = chunk.ChunkIndex,
                [PayloadContent] = chunk.Content
            }
        }).ToList();

        await _qdrantClient.UpsertAsync(
            CollectionName,
            points,
            wait: true,
            cancellationToken: cancellationToken);
    }

    public async Task DeleteByResourceIdAsync(Guid resourceId, CancellationToken cancellationToken)
    {
        if (!await _qdrantClient.CollectionExistsAsync(CollectionName, cancellationToken))
        {
            return;
        }

        await _qdrantClient.DeleteAsync(
            CollectionName,
            MatchKeyword(PayloadResourceId, resourceId.ToString("D")),
            wait: true,
            cancellationToken: cancellationToken);
    }

    public async Task<bool> ExistsByResourceIdAsync(Guid resourceId, CancellationToken cancellationToken)
    {
        if (!await _qdrantClient.CollectionExistsAsync(CollectionName, cancellationToken))
        {
            return false;
        }

        var count = await _qdrantClient.CountAsync(
            CollectionName,
            MatchKeyword(PayloadResourceId, resourceId.ToString("D")),
            exact: true,
            cancellationToken: cancellationToken);

        return count > 0;
    }

    public async Task<IReadOnlyList<ResourceChunkSearchResult>> GetRelevantChunksAsync(
        Guid resourceId,
        string query,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return [];
        }

        var candidates = await SearchVectorCandidatesAsync(
            resourceId,
            query,
            15,
            cancellationToken);

        if (candidates.Count == 0)
        {
            return [];
        }

        try
        {
            var reranked = await RerankCandidatesAsync(query, candidates, cancellationToken);
            if (reranked.Count != 0)
            {
                return reranked;
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogWarning(
                ex,
                "Cohere reranking failed for resource {ResourceId}. Falling back to vector search order.",
                resourceId);
        }

        return candidates.Take(5).ToList();
    }

    private async Task EnsureCollectionAsync(CancellationToken cancellationToken)
    {
        if (_collectionConfigured)
        {
            return;
        }

        await _collectionSetupLock.WaitAsync(cancellationToken);
        try
        {
            if (_collectionConfigured)
            {
                return;
            }

            if (!await _qdrantClient.CollectionExistsAsync(CollectionName, cancellationToken))
            {
                await _qdrantClient.CreateCollectionAsync(
                    CollectionName,
                    new VectorParams
                    {
                        Size = 1536,
                        Distance = Distance.Cosine
                    },
                    cancellationToken: cancellationToken);
            }

            await EnsureResourceIdPayloadIndexAsync(cancellationToken);
            _collectionConfigured = true;
        }
        finally
        {
            _collectionSetupLock.Release();
        }
    }

    private async Task EnsureResourceIdPayloadIndexAsync(CancellationToken cancellationToken)
    {
        try
        {
            await _qdrantClient.CreatePayloadIndexAsync(
                CollectionName,
                PayloadResourceId,
                PayloadSchemaType.Keyword,
                wait: true,
                cancellationToken: cancellationToken);
        }
        catch (RpcException ex) when (ex.StatusCode == StatusCode.AlreadyExists)
        {
        }
        catch (Exception)
        {
            throw;
        }
    }

    private async Task<IReadOnlyList<ResourceChunkSearchResult>> SearchVectorCandidatesAsync(
        Guid resourceId,
        string query,
        int limit,
        CancellationToken cancellationToken)
    {
        await EnsureCollectionAsync(cancellationToken);

        var embedding = await _embeddingService.GenerateEmbeddingAsync(query, cancellationToken);
        if (embedding.IsEmpty)
        {
            return [];
        }

        var points = await _qdrantClient.SearchAsync(
            CollectionName,
            embedding,
            filter: MatchKeyword(PayloadResourceId, resourceId.ToString("D")),
            limit: (ulong)limit,
            payloadSelector: true,
            vectorsSelector: false,
            cancellationToken: cancellationToken);

        return points
            .Select(point => new ResourceChunkSearchResult(
                Guid.Parse(point.Id.Uuid),
                (int)point.Payload[PayloadChunkIndex].IntegerValue,
                point.Payload[PayloadContent].StringValue,
                point.Score))
            .ToList();
    }

    private async Task<IReadOnlyList<ResourceChunkSearchResult>> RerankCandidatesAsync(
        string query,
        IReadOnlyList<ResourceChunkSearchResult> candidates,
        CancellationToken cancellationToken)
    {
        var documents = candidates.Select(c => c.Content).ToList();

        var response = await _cohereClient.V2.Rerank2Async(
            new V2RerankRequest
            {
                Model = RerankModel,
                Query = query,
                Documents = documents,
                TopN = Math.Min(5, candidates.Count),
            },
            cancellationToken: cancellationToken);

        return response.Results
            .Where(result => result.Index >= 0 && result.Index < candidates.Count)
            .Select(result => candidates[result.Index])
            .ToList();
    }
}
