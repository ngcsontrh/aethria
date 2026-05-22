using Aethria.Application.Abstractions.Identity;
using System.Security.Cryptography;
using System.Text;

namespace Aethria.Infrastructure.Identity;

internal sealed class ApiKeyTokenService : IApiKeyTokenService
{
    private const string Prefix = "ae_";
    private const int RandomCharCount = 32;
    private const string Base62Chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

    public ApiKeyTokenGenerationResult GenerateToken(Guid userId, string email, Guid keyId, DateTimeOffset expiresAt)
    {
        var randomBytes = RandomNumberGenerator.GetBytes(RandomCharCount);
        var randomChars = new char[RandomCharCount];
        for (var i = 0; i < RandomCharCount; i++)
        {
            randomChars[i] = Base62Chars[randomBytes[i] % 62];
        }

        var randomPart = new string(randomChars);
        var plaintext = $"{Prefix}{randomPart}";
        var tokenHash = Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(plaintext)));
        var lastFourChars = plaintext[^4..];

        return new ApiKeyTokenGenerationResult(plaintext, tokenHash, lastFourChars);
    }
}
