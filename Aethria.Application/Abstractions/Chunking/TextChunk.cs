namespace Aethria.Application.Abstractions.Chunking;

public sealed record TextChunk(
    Guid Id,
    int Index,
    string Content,
    int TokenCount,
    int StartOffset,
    int EndOffset);
