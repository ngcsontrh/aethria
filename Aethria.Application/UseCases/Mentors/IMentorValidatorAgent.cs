namespace Aethria.Application.UseCases.Mentors;

public interface IMentorValidatorAgent
{
    Task<MentorInstructionValidationResult> ValidateAsync(
        string instruction,
        CancellationToken cancellationToken);
}

public sealed record MentorInstructionValidationResult(
    bool IsValid,
    string? Reason);
