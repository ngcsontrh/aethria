using System.ComponentModel;
using System.Text.Json.Serialization;

namespace Aethria.Infrastructure.AgentFramework.Roadmap;

internal sealed class RoadmapGenerateResponse
{
    [JsonPropertyName("steps")]
    [Description("List of generated roadmap steps.")]
    public List<GeneratedRoadmapStep> Steps { get; set; } = [];
}

internal sealed class GeneratedRoadmapStep
{
    [JsonPropertyName("stepNumber")]
    [Description("1-based step number in the final roadmap.")]
    public int StepNumber { get; set; }

    [JsonPropertyName("title")]
    public string Title { get; set; } = null!;

    [JsonPropertyName("description")]
    public string Description { get; set; } = null!;

    [JsonPropertyName("learningObjectives")]
    public List<string> LearningObjectives { get; set; } = [];

    [JsonPropertyName("prerequisiteStepNumbers")]
    [Description("Earlier step numbers required before this step.")]
    public List<int> PrerequisiteStepNumbers { get; set; } = [];
}
