namespace Aethria.Domain.ValueObjects;

public record MentorTool
{
    public string Value { get; init; }

    private MentorTool(string value) => Value = value;

    public static readonly MentorTool WebSearch = new("web_search");
    public static readonly MentorTool WebExtract = new("web_extract");

    public static MentorTool FromValue(string value)
    {
        return value.ToLowerInvariant() switch
        {
            "web_search" => WebSearch,
            "web_extract" => WebExtract,
            _ => throw new ArgumentException($"Invalid MentorTool: {value}")
        };
    }
}
