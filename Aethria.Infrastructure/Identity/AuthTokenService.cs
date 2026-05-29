using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Aethria.Infrastructure.Identity;

internal sealed class AuthTokenService : IAuthTokenService
{
    private readonly AuthOptions _options;

    public AuthTokenService(
        IOptions<AuthOptions> options)
    {
        _options = options.Value;
    }

    public AccessTokenResult CreateAccessToken(AuthUser user)
    {
        var expiresAt = DateTimeOffset.UtcNow.AddMinutes(_options.AccessTokenMinutes);
        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SigningKey));
        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new(ClaimTypes.Email, user.Email)
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Issuer = _options.Issuer,
            Audience = _options.Audience,
            Subject = new ClaimsIdentity(claims),
            NotBefore = DateTime.UtcNow,
            Expires = expiresAt.UtcDateTime,
            SigningCredentials = credentials
        };

        return new AccessTokenResult(
            new JsonWebTokenHandler().CreateToken(tokenDescriptor),
            expiresAt);
    }

    public RefreshTokenResult CreateRefreshToken(AuthUser user)
    {
        var now = DateTimeOffset.UtcNow;
        var expiresAt = now.AddDays(_options.RefreshTokenDays);
        var tokenId = Guid.NewGuid();
        var randomBytes = RandomNumberGenerator.GetBytes(32);
        var refreshToken = string.Join(
            '.',
            Base64UrlEncoder.Encode(tokenId.ToByteArray()),
            Base64UrlEncoder.Encode(randomBytes));

        return new RefreshTokenResult(user.UserId, tokenId, refreshToken, ComputeTokenHash(refreshToken), expiresAt);
    }

    public bool TryGetRefreshTokenId(string refreshToken, out Guid refreshTokenId)
    {
        refreshTokenId = Guid.Empty;

        var tokenParts = refreshToken.Split('.');
        if (tokenParts.Length != 2)
        {
            return false;
        }

        var refreshTokenIdBytes = Base64UrlEncoder.DecodeBytes(tokenParts[0]);
        if (refreshTokenIdBytes.Length != 16)
        {
            return false;
        }

        refreshTokenId = new Guid(refreshTokenIdBytes);
        return true;
    }

    public bool RefreshTokenMatches(string storedHash, string refreshToken)
    {
        var computedHash = ComputeTokenHash(refreshToken);
        var computedHashBytes = Convert.FromBase64String(computedHash);
        var storedHashBytes = Convert.FromBase64String(storedHash);

        return storedHashBytes.Length == computedHashBytes.Length
            && CryptographicOperations.FixedTimeEquals(computedHashBytes, storedHashBytes);
    }

    private static string ComputeTokenHash(string refreshToken)
    {
        return Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(refreshToken)));
    }
}
