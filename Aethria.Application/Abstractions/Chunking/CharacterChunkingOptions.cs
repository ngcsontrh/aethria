namespace Aethria.Application.Abstractions.Chunking;

public sealed class CharacterChunkingOptions
{
    public int MaxCharactersPerChunk { get; init; } = 4_000;

    public int OverlapCharacters { get; init; } = 600;

    public bool TrimChunks { get; init; } = true;

    public bool RemoveEmptyChunks { get; init; } = true;
}
