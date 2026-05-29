using Microsoft.ML.Tokenizers;

namespace Aethria.Infrastructure.Chunking;

internal sealed class OpenAITokenCountingService : ITokenCountingService
{
    private static readonly Lazy<TiktokenTokenizer> Cl100KBaseTokenizer =
        new(() => TiktokenTokenizer.CreateForEncoding(TokenEncodingNames.Cl100KBase));

    private static readonly Lazy<TiktokenTokenizer> O200KBaseTokenizer =
        new(() => TiktokenTokenizer.CreateForEncoding(TokenEncodingNames.O200KBase));

    public int CountTokens(string text, string encodingName)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return 0;
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(encodingName);

        var tokenizer = encodingName switch
        {
            TokenEncodingNames.Cl100KBase => Cl100KBaseTokenizer.Value,
            TokenEncodingNames.O200KBase => O200KBaseTokenizer.Value,
            _ => throw new ArgumentOutOfRangeException(
                nameof(encodingName),
                encodingName,
                "Unsupported tokenizer encoding.")
        };

        return tokenizer.CountTokens(text);
    }
}
