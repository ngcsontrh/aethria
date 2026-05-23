namespace Aethria.Application.Abstractions.VectorSearch;

public sealed record ResourceChunkVectorInput(
    Guid Id,
    Guid ResourceId,
    int ChunkIndex,
    string Content,
    ReadOnlyMemory<float> Embedding);
