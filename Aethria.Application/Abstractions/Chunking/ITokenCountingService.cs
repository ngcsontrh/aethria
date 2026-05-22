namespace Aethria.Application.Abstractions.Chunking;

public interface ITokenCountingService
{
    int CountTokens(string text, string encodingName);
}
