namespace Aethria.Domain.ValueObjects;

public record NotificationType
{
    public string Value { get; init; }

    private NotificationType(string value) => Value = value;

    public static readonly NotificationType QuizGenerated = new("quiz.generated");
    public static readonly NotificationType RoadmapGenerated = new("roadmap.generated");
    public static readonly NotificationType ResourceCreated = new("resource.created");

    public static NotificationType FromValue(string value)
    {
        return value.ToLowerInvariant() switch
        {
            "quiz.generated" => QuizGenerated,
            "roadmap.generated" => RoadmapGenerated,
            "resource.created" => ResourceCreated,
            _ => throw new ArgumentException($"Invalid NotificationType: {value}")
        };
    }
}
