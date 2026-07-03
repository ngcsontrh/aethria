using System.ComponentModel;
using System.Text.Json;
using Microsoft.Agents.AI;

namespace Aethria.Infrastructure.AgentFramework.Quiz.Skills;

internal sealed class QuizReviewerSkill : AgentClassSkill<QuizReviewerSkill>
{
    public override AgentSkillFrontmatter Frontmatter { get; } = new(
        "quiz-reviewer",
        "Domain expertise for reviewing and validating generated quiz questions. " +
        "Use when evaluating questions for grounding accuracy, quality issues, and option validity.");

    protected override string Instructions => """
        Use this skill when reviewing generated quiz questions to ensure quality and correctness.

        1. Read the review-criteria resource to understand what constitutes an approved vs rejected question.
        2. Read the common-issues resource to identify frequent problems in generated questions.
        3. Use the check-grounding script to verify a question's answer is supported by the source chunks.
        4. Use the validate-options script to check option quality (balance, plausibility, mutual exclusivity).
        """;

    [AgentSkillResource("review-criteria")]
    [Description("Criteria for approving or rejecting generated quiz questions during review.")]
    public string ReviewCriteria => """
        # Quiz Review Criteria

        ## Approval Requirements (ALL must be met)
        1. **Grounding**: The correct answer is directly supported by the source material chunks.
        2. **Single Correct Answer**: Exactly one option is defensibly correct; no ambiguity.
        3. **Clear Stem**: The question is understandable without seeing the source document.
        4. **Plausible Distractors**: All incorrect options are believable to a learner.
        5. **No Source References**: Question does not mention "the text", "the document", "the passage", etc.
        6. **Correct Index**: The correctOptionIndex actually points to the correct answer.
        7. **Valid Explanation**: Explanation justifies the answer without quoting the source.

        ## Rejection Triggers (ANY one triggers needs_revision)
        - Correct answer contradicts or is not found in the retrieved chunks.
        - Multiple options could be argued as correct.
        - Question is unanswerable without reading the source document directly.
        - Question contains document-framing language.
        - One or more distractors are obviously wrong (not plausible).
        - Options overlap in meaning or are not mutually exclusive.
        - Explanation is missing, wrong, or cites the source.
        - Language mismatch between question and source chunks.

        ## Severity Levels
        | Level | Action | Examples |
        |---|---|---|
        | Critical | Always reject | Wrong answer, ungrounded claim, ambiguous correct answer |
        | Major | Reject unless borderline | Document-framing, implausible distractors, overlapping options |
        | Minor | Approve with note | Slightly verbose stem, minor grammar issue |

        ## Review Mindset
        - Be strict on factual accuracy and grounding.
        - Be lenient on minor stylistic preferences.
        - When in doubt about grounding, reject — the editor can fix it with the chunks available.
        - Do not reject a question just because you would phrase it differently.
        """;

    [AgentSkillResource("common-issues")]
    [Description("Catalog of common issues found in AI-generated quiz questions and how to identify them.")]
    public string CommonIssues => """
        # Common Issues in Generated Quiz Questions

        ## Grounding Issues
        - **Hallucinated fact**: Answer states something not present in source chunks.
        - **Over-generalization**: Answer broadens a specific claim from the source.
        - **Temporal drift**: Answer assumes current state when source is time-bound.
        - **Inference leap**: Answer requires assumptions beyond what chunks state.

        ## Structural Issues
        - **Obvious correct answer**: Correct option is significantly longer or more detailed.
        - **Pattern giveaway**: Correct answer always uses qualifiers like "typically" or "generally".
        - **Grammatical mismatch**: Stem ends with "a" but correct answer starts with vowel (or similar).
        - **Stem includes answer**: Question wording inadvertently reveals the answer.

        ## Distractor Issues
        - **Too easy**: Distractors are from completely different domains.
        - **Too similar**: Two options mean essentially the same thing.
        - **Partially correct**: A distractor is also correct in certain contexts.
        - **Absurd option**: One distractor is obviously a joke or impossible.

        ## Document-Framing Patterns to Detect
        - "According to the text/document/source/passage/article/lesson"
        - "The author states/mentions/argues/suggests"
        - "Based on the reading/material/content"
        - "In the passage/text/document above"
        - "As described in the source"
        - Equivalent phrases in any language

        ## Explanation Issues
        - **Circular**: "A is correct because it is the right answer."
        - **Citation**: "As stated on page 3..." or "The source mentions..."
        - **Missing justification**: States the answer without explaining why.
        - **Distractor neglect**: Does not address why tempting wrong answers are wrong.
        """;

    [AgentSkillScript("check-grounding")]
    [Description("Checks whether a question's correct answer can be grounded in the provided source chunks. Returns 'grounded', 'partially-grounded', or 'ungrounded' with reasoning.")]
    public static string CheckGrounding(string questionText, string correctAnswer, string chunksJson)
    {
        // Structural pre-check before the AI does semantic grounding
        if (string.IsNullOrWhiteSpace(questionText))
            return JsonSerializer.Serialize(new { status = "ungrounded", reason = "Question text is empty." });

        if (string.IsNullOrWhiteSpace(correctAnswer))
            return JsonSerializer.Serialize(new { status = "ungrounded", reason = "Correct answer text is empty." });

        List<string>? chunks = null;
        try
        {
            chunks = JsonSerializer.Deserialize<List<string>>(chunksJson);
        }
        catch
        {
            return JsonSerializer.Serialize(new { status = "ungrounded", reason = "Chunks JSON is not a valid string array." });
        }

        if (chunks is null || chunks.Count == 0)
            return JsonSerializer.Serialize(new { status = "ungrounded", reason = "No source chunks provided — grounding cannot be verified." });

        // Simple keyword overlap heuristic as a structural signal
        var answerWords = correctAnswer
            .Split([' ', ',', '.', ';', ':', '!', '?', '-', '(', ')', '"', '\''], StringSplitOptions.RemoveEmptyEntries)
            .Where(w => w.Length > 3)
            .Select(w => w.ToLowerInvariant())
            .ToHashSet();

        if (answerWords.Count == 0)
            return JsonSerializer.Serialize(new { status = "partially-grounded", reason = "Answer is too short for keyword overlap analysis." });

        var allChunkText = string.Join(" ", chunks).ToLowerInvariant();
        var matchedWords = answerWords.Count(w => allChunkText.Contains(w));
        var overlapRatio = (double)matchedWords / answerWords.Count;

        if (overlapRatio >= 0.6)
            return JsonSerializer.Serialize(new { status = "grounded", reason = $"Strong keyword overlap ({matchedWords}/{answerWords.Count} key terms found in chunks)." });

        if (overlapRatio >= 0.3)
            return JsonSerializer.Serialize(new { status = "partially-grounded", reason = $"Moderate keyword overlap ({matchedWords}/{answerWords.Count} key terms). Semantic review recommended." });

        return JsonSerializer.Serialize(new { status = "ungrounded", reason = $"Low keyword overlap ({matchedWords}/{answerWords.Count} key terms). Answer may not be supported by source chunks." });
    }

    [AgentSkillScript("validate-options")]
    [Description("Validates option quality: checks for balance, uniqueness, length consistency, and potential anti-patterns. Returns 'valid' or a list of issues.")]
    public static string ValidateOptions(string optionsJson, int correctOptionIndex)
    {
        var issues = new List<string>();

        List<string>? options = null;
        try
        {
            options = JsonSerializer.Deserialize<List<string>>(optionsJson);
        }
        catch
        {
            return JsonSerializer.Serialize(new { valid = false, issues = new[] { "Options JSON is not a valid string array." } });
        }

        if (options is null || options.Count != 4)
        {
            return JsonSerializer.Serialize(new { valid = false, issues = new[] { $"Expected 4 options, got {options?.Count ?? 0}." } });
        }

        // Check for empty options
        for (var i = 0; i < options.Count; i++)
        {
            if (string.IsNullOrWhiteSpace(options[i]))
                issues.Add($"Option {i} is empty.");
        }

        if (issues.Count > 0)
            return JsonSerializer.Serialize(new { valid = false, issues });

        if (correctOptionIndex < 0 || correctOptionIndex > 3)
        {
            return JsonSerializer.Serialize(new { valid = false, issues = new[] { $"correctOptionIndex {correctOptionIndex} is out of range (0-3)." } });
        }

        // Check uniqueness
        var uniqueOptions = options.Select(o => o.Trim().ToLowerInvariant()).Distinct().Count();
        if (uniqueOptions < 4)
            issues.Add("Some options are duplicates or near-duplicates.");

        // Check length balance
        var lengths = options.Select(o => o.Length).ToList();
        var avgLength = lengths.Average();
        var correctLength = lengths[correctOptionIndex];

        if (correctLength > avgLength * 1.8)
            issues.Add("Correct answer is significantly longer than other options (potential giveaway).");

        var maxLength = lengths.Max();
        var minLength = lengths.Min();
        if (maxLength > minLength * 3 && minLength > 0)
            issues.Add("Options have highly uneven lengths — consider rebalancing.");

        // Check for "All of the above" / "None of the above" anti-patterns
        var antiPatterns = new[] { "all of the above", "none of the above", "both a and b", "a and b", "b and c" };
        foreach (var option in options)
        {
            var lower = option.ToLowerInvariant().Trim();
            if (antiPatterns.Any(p => lower.Contains(p)))
                issues.Add($"Option contains anti-pattern phrase: \"{option}\"");
        }

        // Check for absolute qualifiers only in the correct answer
        var absoluteQualifiers = new[] { "always", "never", "all", "none", "every", "only" };
        var correctHasAbsolute = absoluteQualifiers.Any(q => options[correctOptionIndex].ToLowerInvariant().Contains(q));
        var othersHaveAbsolute = options
            .Where((_, i) => i != correctOptionIndex)
            .Any(o => absoluteQualifiers.Any(q => o.ToLowerInvariant().Contains(q)));

        if (correctHasAbsolute && !othersHaveAbsolute)
            issues.Add("Only the correct answer uses absolute qualifiers — this may create a pattern.");

        return JsonSerializer.Serialize(new { valid = issues.Count == 0, issues });
    }
}
