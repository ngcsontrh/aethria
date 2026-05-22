namespace Aethria.Application.Abstractions.Chunking;

public sealed class ChunkingOptions
{
    public string EncodingName { get; init; } = TokenEncodingNames.Cl100KBase;

    public int MaxTokensPerLine { get; init; } = 300;

    public int MaxTokensPerChunk { get; init; } = 800;

    public int OverlapTokens { get; init; } = 120;

    public bool TrimChunks { get; init; } = true;

    public bool RemoveEmptyChunks { get; init; } = true;
}
