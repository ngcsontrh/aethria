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
