#pragma warning disable SKEXP0050

using Aethria.Application.Abstractions.Chunking;
using Microsoft.SemanticKernel.Text;

namespace Aethria.Infrastructure.Chunking;

internal sealed class OpenAITextChunkingService : ITextChunkingService
{
    private readonly ITokenCountingService _tokenCountingService;

    public OpenAITextChunkingService(ITokenCountingService tokenCountingService)
    {
        _tokenCountingService = tokenCountingService;
    }

    public Task<IReadOnlyList<TextChunk>> ChunkTextAsync(
        string content,
        ChunkingOptions options,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        ArgumentException.ThrowIfNullOrWhiteSpace(content);

        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(options.MaxTokensPerLine);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(options.MaxTokensPerChunk);
        ArgumentOutOfRangeException.ThrowIfNegative(options.OverlapTokens);

        if (options.OverlapTokens >= options.MaxTokensPerChunk)
        {
            throw new ArgumentException("OverlapTokens must be smaller than MaxTokensPerChunk.");
        }

        int tokenCounter(string input) => _tokenCountingService.CountTokens(input, options.EncodingName);

        var normalizedContent = content
            .Replace("\r\n", "\n")
            .Replace('\r', '\n');

        var lines = TextChunker.SplitPlainTextLines(
            normalizedContent,
            maxTokensPerLine: options.MaxTokensPerLine,
            tokenCounter: tokenCounter);

        var rawChunks = TextChunker.SplitPlainTextParagraphs(
            lines,
            maxTokensPerParagraph: options.MaxTokensPerChunk,
            overlapTokens: options.OverlapTokens,
            tokenCounter: tokenCounter);

        var result = new List<TextChunk>(rawChunks.Count);

        var searchStart = 0;

        for (var index = 0; index < rawChunks.Count; index++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var chunk = rawChunks[index];

            if (options.TrimChunks)
            {
                chunk = chunk.Trim();
            }

            if (options.RemoveEmptyChunks && string.IsNullOrWhiteSpace(chunk))
            {
                continue;
            }

            searchStart = Math.Min(searchStart, normalizedContent.Length);

            var startOffset = normalizedContent.IndexOf(
                chunk,
                searchStart,
                StringComparison.Ordinal);

            if (startOffset < 0)
            {
                startOffset = searchStart;
            }

            var endOffset = startOffset + chunk.Length;

            searchStart = Math.Max(endOffset, searchStart);

            result.Add(new TextChunk(
                Id: Guid.CreateVersion7(),
                Index: result.Count,
                Content: chunk,
                TokenCount: tokenCounter(chunk),
                StartOffset: startOffset,
                EndOffset: endOffset));
        }

        return Task.FromResult<IReadOnlyList<TextChunk>>(result);
    }

    public Task<IReadOnlyList<TextChunk>> ChunkTextByCharactersAsync(
        string content,
        CharacterChunkingOptions options,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        ArgumentException.ThrowIfNullOrWhiteSpace(content);

        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(options.MaxCharactersPerChunk);
        ArgumentOutOfRangeException.ThrowIfNegative(options.OverlapCharacters);

        if (options.OverlapCharacters >= options.MaxCharactersPerChunk)
        {
            throw new ArgumentException("OverlapCharacters must be smaller than MaxCharactersPerChunk.");
        }

        var normalizedContent = content
            .Replace("\r\n", "\n")
            .Replace('\r', '\n');

        var normalized = options.TrimChunks
            ? normalizedContent.Trim()
            : normalizedContent;

        if (options.RemoveEmptyChunks && string.IsNullOrWhiteSpace(normalized))
        {
            return Task.FromResult<IReadOnlyList<TextChunk>>([]);
        }

        var result = new List<TextChunk>();
        var start = 0;
        var searchStart = 0;

        while (start < normalized.Length)
        {
            cancellationToken.ThrowIfCancellationRequested();

            while (start < normalized.Length && char.IsWhiteSpace(normalized[start]))
            {
                start++;
            }

            if (start >= normalized.Length)
            {
                break;
            }

            var preferredEnd = Math.Min(start + options.MaxCharactersPerChunk, normalized.Length);
            var end = preferredEnd;

            if (preferredEnd < normalized.Length)
            {
                var minimum = start + Math.Max(1, options.MaxCharactersPerChunk / 2);

                for (var i = preferredEnd - 1; i > minimum; i--)
                {
                    if (normalized[i] == '\n' && i > start && normalized[i - 1] == '\n')
                    {
                        end = i - 1;
                        break;
                    }
                }

                if (end == preferredEnd)
                {
                    for (var i = preferredEnd - 1; i > minimum; i--)
                    {
                        if (char.IsWhiteSpace(normalized[i]))
                        {
                            end = i;
                            break;
                        }
                    }
                }
            }

            if (end <= start)
            {
                end = preferredEnd;
            }

            var chunk = normalized[start..end];

            if (options.TrimChunks)
            {
                chunk = chunk.Trim();
            }

            if (!options.RemoveEmptyChunks || !string.IsNullOrWhiteSpace(chunk))
            {
                searchStart = Math.Min(searchStart, normalizedContent.Length);

                var startOffset = normalizedContent.IndexOf(
                    chunk,
                    searchStart,
                    StringComparison.Ordinal);

                if (startOffset < 0)
                {
                    startOffset = Math.Max(0, searchStart - options.OverlapCharacters);
                    startOffset = normalizedContent.IndexOf(
                        chunk,
                        startOffset,
                        StringComparison.Ordinal);
                }

                if (startOffset < 0)
                {
                    startOffset = searchStart;
                }

                var endOffset = startOffset + chunk.Length;
                searchStart = Math.Max(startOffset + Math.Max(1, chunk.Length - options.OverlapCharacters), searchStart);

                result.Add(new TextChunk(
                    Id: Guid.CreateVersion7(),
                    Index: result.Count,
                    Content: chunk,
                    TokenCount: _tokenCountingService.CountTokens(chunk, TokenEncodingNames.Cl100KBase),
                    StartOffset: startOffset,
                    EndOffset: endOffset));
            }

            if (end >= normalized.Length)
            {
                break;
            }

            if (options.OverlapCharacters <= 0)
            {
                start = end;
                continue;
            }

            var nextStart = Math.Max(start + 1, end - options.OverlapCharacters);
            for (var i = nextStart; i < end; i++)
            {
                if (char.IsWhiteSpace(normalized[i]))
                {
                    nextStart = i + 1;
                    break;
                }
            }

            start = nextStart > start ? nextStart : end;
        }

        return Task.FromResult<IReadOnlyList<TextChunk>>(result);
    }
}
