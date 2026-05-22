namespace Aethria.Domain.Entities;

public class ApiKey : BaseEntity
{
    public Guid UserId { get; set; }
    public string Name { get; set; } = null!;
    public string TokenHash { get; set; } = null!;
    public DateTimeOffset ExpiresAt { get; set; }
    public ApiKeyStatus Status { get; set; } = ApiKeyStatus.Active;
    public DateTimeOffset? RevokedAt { get; set; }
    public string LastFourChars { get; set; } = null!;
}
