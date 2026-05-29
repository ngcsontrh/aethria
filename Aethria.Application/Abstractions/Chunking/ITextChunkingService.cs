namespace Aethria.Application.Abstractions.Chunking;

public interface ITextChunkingService
{
    Task<IReadOnlyList<TextChunk>> ChunkTextAsync(
        string content,
        ChunkingOptions options,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<TextChunk>> ChunkTextByCharactersAsync(
        string content,
        CharacterChunkingOptions options,
        CancellationToken cancellationToken);
}

public sealed record TextChunk(
    Guid Id,
    int Index,
    string Content,
    int TokenCount,
    int StartOffset,
    int EndOffset);

public sealed class ChunkingOptions
{
    public string EncodingName { get; init; } = TokenEncodingNames.Cl100KBase;

    public int MaxTokensPerLine { get; init; } = 300;

    public int MaxTokensPerChunk { get; init; } = 1_000;

    public int OverlapTokens { get; init; } = 200;

    public bool TrimChunks { get; init; } = true;

    public bool RemoveEmptyChunks { get; init; } = true;
}

public sealed class CharacterChunkingOptions
{
    public int MaxCharactersPerChunk { get; init; } = 4_000;

    public int OverlapCharacters { get; init; } = 600;

    public bool TrimChunks { get; init; } = true;

    public bool RemoveEmptyChunks { get; init; } = true;
}
