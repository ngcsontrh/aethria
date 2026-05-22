namespace Aethria.Application.UseCases.Mentors.UpdateMentor;

internal sealed class UpdateMentorCommandValidator : AbstractValidator<UpdateMentorCommand>
{
    private static readonly HashSet<string> AllowedTools = new(StringComparer.OrdinalIgnoreCase)
    {
        "web_search",
        "web_extract"
    };

    public UpdateMentorCommandValidator()
    {
        RuleFor(command => command.MentorId)
            .NotEmpty()
            .WithMessage("MentorId is required.");

        RuleFor(command => command.Name)
            .Must(name => !string.IsNullOrWhiteSpace(name) && name.Length <= 200)
            .WithMessage("Name must be between 1 and 200 characters.");

        RuleFor(command => command.Description)
            .Must(description => !string.IsNullOrWhiteSpace(description) && description.Length <= 2000)
            .WithMessage("Description must be between 1 and 2000 characters.");

        RuleFor(command => command.Instruction)
            .Must(instruction => !string.IsNullOrWhiteSpace(instruction) && instruction.Length <= 5000)
            .WithMessage("Instruction must be between 1 and 5000 characters.");

        RuleFor(command => command.Tools)
            .Must(tools => tools is null || tools.All(tool => AllowedTools.Contains(tool)))
            .WithMessage("Tools contain invalid values. Allowed tools: web_search, web_extract.");

        RuleFor(command => command.UserId)
            .NotEmpty()
            .WithMessage("UserId is required.");
    }
}
