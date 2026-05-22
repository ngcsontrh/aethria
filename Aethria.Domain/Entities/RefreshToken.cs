namespace Aethria.Domain.Entities;

public class RefreshToken : BaseEntity
{
    public Guid UserId { get; set; }
    public string TokenHash { get; set; } = null!;
    public DateTimeOffset ExpiresAt { get; set; }
    public RefreshTokenStatus Status { get; set; } = RefreshTokenStatus.Active;
    public DateTimeOffset? RevokedAt { get; set; }
}
