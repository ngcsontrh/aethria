namespace Aethria.Domain.ValueObjects;

public record ApiKeyStatus
{
    public string Value { get; init; }

    private ApiKeyStatus(string value) => Value = value;

    public static readonly ApiKeyStatus Active = new("active");
    public static readonly ApiKeyStatus Revoked = new("revoked");

    public static ApiKeyStatus FromValue(string value)
    {
        return value.ToLowerInvariant() switch
        {
            "active" => Active,
            "revoked" => Revoked,
            _ => throw new ArgumentException($"Invalid ApiKeyStatus: {value}")
        };
    }
}
