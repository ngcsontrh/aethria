namespace Aethria.Application.Abstractions.VectorSearch;

public sealed record ResourceChunkSearchResult(
    Guid Id,
    int ChunkIndex,
    string Content,
    double VectorScore);
