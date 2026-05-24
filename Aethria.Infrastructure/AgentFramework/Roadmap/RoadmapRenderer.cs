using System.Text;

namespace Aethria.Infrastructure.AgentFramework.Roadmap;

internal static class RoadmapRenderer
{
    public static string RenderMarkdown(IReadOnlyList<GeneratedRoadmapStep> steps)
    {
        var orderedSteps = steps.OrderBy(s => s.StepNumber).ToList();
        var sb = new StringBuilder();

        foreach (var step in orderedSteps)
        {
            if (sb.Length > 0)
            {
                sb.AppendLine();
            }

            sb.AppendLine($"## {step.StepNumber}. {step.Title.Trim()}");
            sb.AppendLine();
            sb.AppendLine(step.Description.Trim());

            var learningObjectives = step.LearningObjectives
                .Where(o => !string.IsNullOrWhiteSpace(o))
                .Select(o => o.Trim())
                .ToList();

            if (learningObjectives.Count > 0)
            {
                sb.AppendLine();

                foreach (var objective in learningObjectives)
                {
                    sb.AppendLine($"- {objective}");
                }
            }
        }

        return sb.ToString().TrimEnd();
    }

    public static string RenderMermaid(IReadOnlyList<GeneratedRoadmapStep> steps)
    {
        var orderedSteps = steps.OrderBy(s => s.StepNumber).ToList();
        var stepNumbers = orderedSteps.Select(s => s.StepNumber).ToHashSet();
        var sb = new StringBuilder();

        sb.AppendLine("flowchart TD");

        foreach (var step in orderedSteps)
        {
            sb.AppendLine($"    node_{step.StepNumber}[\"{EscapeMermaidLabel(step.Title)}\"]");
        }

        foreach (var step in orderedSteps)
        {
            foreach (var prerequisite in step.PrerequisiteStepNumbers.Distinct().OrderBy(n => n))
            {
                if (stepNumbers.Contains(prerequisite) && prerequisite < step.StepNumber)
                {
                    sb.AppendLine($"    node_{prerequisite} --> node_{step.StepNumber}");
                }
            }
        }

        return sb.ToString().TrimEnd();
    }

    private static string EscapeMermaidLabel(string value) =>
        value
            .Trim()
            .Replace("\\", "\\\\")
            .Replace("\"", "\\\"");
}
