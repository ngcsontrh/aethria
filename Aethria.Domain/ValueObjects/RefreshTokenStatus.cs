namespace Aethria.Domain.ValueObjects;

public record RefreshTokenStatus
{
    public string Value { get; init; }

    private RefreshTokenStatus(string value) => Value = value;

    public static readonly RefreshTokenStatus Active = new("active");
    public static readonly RefreshTokenStatus Revoked = new("revoked");

    public static RefreshTokenStatus FromValue(string value)
    {
        return value.ToLowerInvariant() switch
        {
            "active" => Active,
            "revoked" => Revoked,
            _ => throw new ArgumentException($"Invalid RefreshTokenStatus: {value}")
        };
    }
}
