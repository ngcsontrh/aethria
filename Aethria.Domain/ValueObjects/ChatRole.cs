namespace Aethria.Domain.ValueObjects;

public record ChatRole
{
    public string Value { get; init; }

    private ChatRole(string value) => Value = value;

    public static readonly ChatRole User = new("user");
    public static readonly ChatRole Assistant = new("assistant");
    public static readonly ChatRole System = new("system");
    public static readonly ChatRole Tool = new("tool");

    public static ChatRole FromValue(string value)
    {
        return value.ToLowerInvariant() switch
        {
            "user" => User,
            "assistant" => Assistant,
            "system" => System,
            "tool" => Tool,
            _ => throw new ArgumentException($"Invalid ChatRole: {value}")
        };
    }
}
