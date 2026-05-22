namespace Aethria.Infrastructure.Configuration;

internal sealed class AuthOptions
{
    public const string SectionName = "Auth";

    public required string Issuer { get; set; }
    public required string Audience { get; set; }
    public required string SigningKey { get; set; }
    public required int AccessTokenMinutes { get; set; }
    public required int RefreshTokenDays { get; set; }
    public required string GoogleClientId { get; set; }
}
