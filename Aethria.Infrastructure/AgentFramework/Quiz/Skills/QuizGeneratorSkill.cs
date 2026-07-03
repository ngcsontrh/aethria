using System.ComponentModel;
using System.Text.Json;
using Microsoft.Agents.AI;

namespace Aethria.Infrastructure.AgentFramework.Quiz.Skills;

internal sealed class QuizGeneratorSkill : AgentClassSkill<QuizGeneratorSkill>
{
    public override AgentSkillFrontmatter Frontmatter { get; } = new(
        "quiz-generator",
        "Domain expertise for generating high-quality multiple-choice quiz questions. " +
        "Use when creating quiz questions to select appropriate difficulty levels, question types, and topic focus strategies.");

    protected override string Instructions => """
        Use this skill when generating quiz questions to ensure quality and variety.

        1. Read the difficulty-levels resource to determine the appropriate complexity for each question.
        2. Read the question-types resource to vary the style of questions generated.
        3. Read the generation-best-practices resource for rules on writing clear, unambiguous questions.
        4. Use the validate-question-structure script to verify each generated question meets structural requirements before returning.
        """;

    [AgentSkillResource("difficulty-levels")]
    [Description("Definitions of difficulty levels and how to apply them when generating questions.")]
    public string DifficultyLevels => """
        # Quiz Difficulty Levels

        ## Easy (Knowledge / Recall)
        - Tests direct recall of facts, definitions, or simple concepts.
        - The correct answer is explicitly stated in the source material.
        - Distractors are clearly different from the correct answer.
        - Suitable for: introductory concepts, terminology, basic facts.
        - Example pattern: "What is [term]?" / "Which of the following is [definition]?"

        ## Medium (Comprehension / Application)
        - Tests understanding of relationships, cause-effect, or application of concepts.
        - The correct answer requires connecting two or more pieces of information.
        - Distractors are plausible but represent common misconceptions.
        - Suitable for: process understanding, comparisons, applying rules to scenarios.
        - Example pattern: "Why does [X] happen?" / "In which scenario would [concept] apply?"

        ## Hard (Analysis / Evaluation)
        - Tests ability to analyze, compare, evaluate, or synthesize information.
        - The correct answer requires reasoning across multiple concepts or edge cases.
        - Distractors are sophisticated and represent partial truths or subtle errors.
        - Suitable for: trade-off analysis, exception identification, multi-step reasoning.
        - Example pattern: "Which approach is most appropriate when [complex scenario]?" / "What distinguishes [X] from [Y] in the context of [Z]?"

        ## Distribution Guidelines
        - Default distribution: 30% Easy, 50% Medium, 20% Hard.
        - When user specifies a difficulty preference, shift distribution accordingly.
        - Never generate all questions at the same difficulty — always include at least 2 levels.
        - Ensure harder questions still have a single defensible correct answer.
        """;

    [AgentSkillResource("question-types")]
    [Description("Catalog of question types to vary quiz generation and maximize pedagogical value.")]
    public string QuestionTypes => """
        # Question Types for Multiple-Choice Quizzes

        ## Factual
        - Tests recall of specific facts, names, numbers, or definitions.
        - Best for: terminology, key facts, straightforward concepts.
        - Keep question stem short and direct.

        ## Conceptual
        - Tests understanding of ideas, principles, or relationships between concepts.
        - Best for: explaining why something works, distinguishing related concepts.
        - Question stem describes a concept and asks for its implication or meaning.

        ## Applied / Scenario-Based
        - Presents a realistic situation and asks the learner to apply knowledge.
        - Best for: procedures, decision-making, problem-solving.
        - Question stem includes a brief scenario (2-3 sentences max).

        ## Comparative
        - Asks learners to identify differences or similarities between two or more items.
        - Best for: distinguishing similar concepts, evaluating trade-offs.
        - Use "Which of the following best distinguishes..." or "How does X differ from Y?"

        ## Cause-and-Effect
        - Tests understanding of why something happens or what results from an action.
        - Best for: processes, consequences, dependencies.
        - Use "What happens when..." or "What is the result of..."

        ## Negation / Exception
        - Asks which option does NOT apply or is an exception to a rule.
        - Best for: testing thorough understanding by identifying the odd one out.
        - Always bold or capitalize NOT/EXCEPT in the question stem for clarity.

        ## Ordering / Sequence
        - Tests knowledge of correct order or priority of steps.
        - Best for: processes, protocols, hierarchies.
        - Options represent different orderings; only one is correct.

        ## Variety Guidelines
        - Do not use the same question type for all questions in a batch.
        - Factual and Conceptual should cover at most 50% combined.
        - Include at least one Applied/Scenario-Based question per 5 questions.
        - Negation questions should be used sparingly (max 1 per 5 questions).
        """;

    [AgentSkillResource("generation-best-practices")]
    [Description("Rules and anti-patterns for writing high-quality multiple-choice questions.")]
    public string GenerationBestPractices => """
        # Quiz Generation Best Practices

        ## Question Stem Rules
        - Write one clear, complete sentence that forms a question or statement to complete.
        - Avoid double negatives.
        - Do not include unnecessary context or filler words.
        - Place qualifiers (always, never, most, least) clearly and intentionally.
        - Never reference the source document ("according to the text", "in the passage").

        ## Option Rules
        - All 4 options must be grammatically consistent with the stem.
        - Options should be similar in length (±30% character count).
        - Avoid "All of the above" and "None of the above".
        - Options must be mutually exclusive — no overlapping answers.
        - Randomize correct answer position across questions (don't always use option A or D).
        - Distractors must be plausible for someone who partially understands the topic.

        ## Anti-Patterns to Avoid
        | Anti-Pattern | Why It's Bad |
        |---|---|
        | Trivially obvious correct answer | Does not test understanding |
        | Joke or absurd distractor | Reduces effective option count |
        | Two options meaning the same thing | Eliminates one distractor |
        | Correct answer is longest option | Gives away the answer by length |
        | Using "always" or "never" in only one option | Pattern gives away the answer |
        | Verbatim copy from source | Tests recognition, not understanding |

        ## Explanation Rules
        - Explain WHY the correct answer is right, not just WHAT it is.
        - Briefly mention why the most tempting distractor is wrong.
        - Keep explanations to 2-3 sentences maximum.
        - Do not quote or cite the source material in explanations.

        ## Language and Tone
        - Match the language of the source content.
        - Use professional, educational tone.
        - Avoid colloquialisms unless the source uses them.
        - Be precise with technical terminology.
        """;

    [AgentSkillScript("validate-question-structure")]
    [Description("Validates that a generated question meets structural requirements. Returns 'valid' or a description of structural issues found.")]
    public static string ValidateQuestionStructure(string questionText, string optionsJson, int correctOptionIndex, string explanation)
    {
        var issues = new List<string>();

        // Validate question text
        if (string.IsNullOrWhiteSpace(questionText))
        {
            issues.Add("questionText is empty.");
        }
        else
        {
            if (questionText.Length < 10)
                issues.Add("questionText is too short (minimum 10 characters).");
            if (questionText.Length > 500)
                issues.Add("questionText is too long (maximum 500 characters).");
        }

        // Validate options
        List<string>? options = null;
        try
        {
            options = JsonSerializer.Deserialize<List<string>>(optionsJson);
        }
        catch
        {
            issues.Add("options is not valid JSON array of strings.");
        }

        if (options is not null)
        {
            if (options.Count != 4)
                issues.Add($"Expected exactly 4 options, got {options.Count}.");

            if (options.Any(string.IsNullOrWhiteSpace))
                issues.Add("One or more options are empty.");

            if (options.Count == 4 && options.Distinct(StringComparer.OrdinalIgnoreCase).Count() < 4)
                issues.Add("Options are not all unique.");

            // Check length balance
            if (options.Count == 4 && options.All(o => !string.IsNullOrWhiteSpace(o)))
            {
                var lengths = options.Select(o => o.Length).ToList();
                var avg = lengths.Average();
                if (lengths.Any(l => l > avg * 2.5))
                    issues.Add("Option lengths are unbalanced — one option is significantly longer than others.");
            }
        }

        // Validate correctOptionIndex
        if (correctOptionIndex < 0 || correctOptionIndex > 3)
            issues.Add($"correctOptionIndex must be 0-3, got {correctOptionIndex}.");

        // Validate explanation
        if (string.IsNullOrWhiteSpace(explanation))
            issues.Add("explanation is empty.");
        else if (explanation.Length < 20)
            issues.Add("explanation is too short (minimum 20 characters).");

        return issues.Count == 0 ? "valid" : string.Join(" | ", issues);
    }
}
