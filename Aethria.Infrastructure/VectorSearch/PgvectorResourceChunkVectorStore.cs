using Pgvector;
using Pgvector.EntityFrameworkCore;
using Cohere;
using Microsoft.Extensions.Logging;

namespace Aethria.Infrastructure.VectorSearch;

internal sealed class PgvectorResourceChunkVectorStore : IResourceChunkVectorStore
{
    private const int CandidateLimit = 10;
    private const int MaxTokensPerDocument = 256;
    private const int CohereRequestsPerMinute = 20;
    private const int CohereTokensPerMinute = 20_000;
    private const int ApproximateCharactersPerToken = 4;
    private const string RerankModel = "Cohere-rerank-v4.0-fast-1";
    private static readonly SemaphoreSlim _cohereRateLimitLock = new(1, 1);
    private static DateTimeOffset _nextCohereRequestUtc = DateTimeOffset.MinValue;

    private readonly AppDbContext _dbContext;
    private readonly IEmbeddingService _embeddingService;
    private readonly CohereClient _cohereClient;
    private readonly ILogger<PgvectorResourceChunkVectorStore> _logger;

    public PgvectorResourceChunkVectorStore(
        AppDbContext dbContext,
        IEmbeddingService embeddingService,
        CohereClient cohereClient,
        ILogger<PgvectorResourceChunkVectorStore> logger)
    {
        _dbContext = dbContext;
        _embeddingService = embeddingService;
        _cohereClient = cohereClient;
        _logger = logger;
    }

    public async Task UpsertAsync(
        Guid resourceId,
        IReadOnlyList<ResourceChunkVectorInput> chunks,
        CancellationToken cancellationToken)
    {
        await DeleteByResourceIdAsync(resourceId, cancellationToken);

        if (chunks.Count == 0)
        {
            return;
        }

        var now = DateTimeOffset.UtcNow;
        var entities = chunks.Select(chunk => new ResourceChunk
        {
            Id = chunk.Id,
            ResourceId = chunk.ResourceId,
            ChunkIndex = chunk.ChunkIndex,
            Content = chunk.Content,
            Embedding = chunk.Embedding,
            CreatedAt = now,
            UpdatedAt = now
        });

        await _dbContext.ResourceChunks.AddRangeAsync(entities, cancellationToken);
    }

    public async Task DeleteByResourceIdAsync(Guid resourceId, CancellationToken cancellationToken)
    {
        await _dbContext.ResourceChunks
            .Where(c => c.ResourceId == resourceId)
            .ExecuteDeleteAsync(cancellationToken);
    }

    public async Task<bool> ExistsByResourceIdAsync(Guid resourceId, CancellationToken cancellationToken)
    {
        return await _dbContext.ResourceChunks
            .AnyAsync(c => c.ResourceId == resourceId, cancellationToken);
    }

    public async Task<IReadOnlyList<ResourceChunkSearchResult>> GetRelevantChunksAsync(
        Guid resourceId,
        string query,
        int topK,
        CancellationToken cancellationToken)
    {
        if (topK <= 0 || string.IsNullOrWhiteSpace(query))
        {
            return [];
        }

        var candidates = await SearchVectorCandidatesAsync(
            resourceId,
            query,
            Math.Max(topK, CandidateLimit),
            cancellationToken);

        if (candidates.Count == 0)
        {
            return [];
        }

        try
        {
            var reranked = await RerankCandidatesAsync(query, candidates, topK, cancellationToken);
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

        return candidates.Take(topK).ToList();
    }

    private async Task<IReadOnlyList<ResourceChunkSearchResult>> SearchVectorCandidatesAsync(
        Guid resourceId,
        string query,
        int limit,
        CancellationToken cancellationToken)
    {
        var embedding = await _embeddingService.GenerateEmbeddingAsync(query, cancellationToken);
        var vector = new Vector(embedding);

        return await _dbContext.ResourceChunks
            .AsNoTracking()
            .Where(c => c.ResourceId == resourceId && c.Embedding != null)
            .OrderBy(c => c.Embedding!.L2Distance(vector))
            .Take(limit)
            .Select(c => new ResourceChunkSearchResult(
                c.Id,
                c.ChunkIndex,
                c.Content,
                c.Embedding!.L2Distance(vector)))
            .ToListAsync(cancellationToken);
    }

    private async Task<IReadOnlyList<ResourceChunkSearchResult>> RerankCandidatesAsync(
        string query,
        IReadOnlyList<ResourceChunkSearchResult> candidates,
        int topK,
        CancellationToken cancellationToken)
    {
        var documents = candidates.Select(c => c.Content).ToList();
        await WaitForCohereRateLimitAsync(query, documents, cancellationToken);

        var response = await _cohereClient.V2.Rerank2Async(
            new V2RerankRequest
            {
                Model = RerankModel,
                Query = query,
                Documents = documents,
                TopN = Math.Min(topK, candidates.Count),
                MaxTokensPerDoc = MaxTokensPerDocument
            },
            cancellationToken: cancellationToken);

        return response.Results
            .Where(result => result.Index >= 0 && result.Index < candidates.Count)
            .Select(result => candidates[result.Index])
            .ToList();
    }

    private static async Task WaitForCohereRateLimitAsync(
        string query,
        IReadOnlyList<string> documents,
        CancellationToken cancellationToken)
    {
        var estimatedTokens = EstimateRerankTokens(query, documents);
        var requestWindow = TimeSpan.FromMinutes(1d / CohereRequestsPerMinute);
        var tokenWindow = TimeSpan.FromMinutes((double)estimatedTokens / CohereTokensPerMinute);
        var delayBetweenRequests = requestWindow > tokenWindow ? requestWindow : tokenWindow;

        await _cohereRateLimitLock.WaitAsync(cancellationToken);
        try
        {
            var now = DateTimeOffset.UtcNow;
            if (_nextCohereRequestUtc > now)
            {
                await Task.Delay(_nextCohereRequestUtc - now, cancellationToken);
                now = DateTimeOffset.UtcNow;
            }

            _nextCohereRequestUtc = now + delayBetweenRequests;
        }
        finally
        {
            _cohereRateLimitLock.Release();
        }
    }

    private static int EstimateRerankTokens(string query, IReadOnlyList<string> documents)
    {
        var queryTokens = EstimateTextTokens(query);
        var documentTokens = documents.Sum(document => Math.Min(
            EstimateTextTokens(document),
            MaxTokensPerDocument));

        return Math.Max(1, queryTokens + documentTokens);
    }

    private static int EstimateTextTokens(string text)
    {
        return Math.Max(1, (int)Math.Ceiling(text.Length / (double)ApproximateCharactersPerToken));
    }

}
