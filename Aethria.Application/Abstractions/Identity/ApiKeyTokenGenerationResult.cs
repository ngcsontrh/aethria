namespace Aethria.Application.Abstractions.Identity;

public sealed record ApiKeyTokenGenerationResult(
    string Token,
    string TokenHash,
    string LastFourChars);
