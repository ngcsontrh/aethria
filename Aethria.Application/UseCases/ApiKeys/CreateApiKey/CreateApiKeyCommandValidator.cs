namespace Aethria.Application.UseCases.ApiKeys.CreateApiKey;

internal sealed class CreateApiKeyCommandValidator : AbstractValidator<CreateApiKeyCommand>
{
    public CreateApiKeyCommandValidator()
    {
        RuleFor(command => command.UserId)
            .NotEmpty()
            .WithMessage("UserId is required.");

        RuleFor(command => command.Email)
            .NotEmpty()
            .WithMessage("Email is required.");

        RuleFor(command => command.Name)
            .Must(name => !string.IsNullOrWhiteSpace(name) && name.Length is >= 1 and <= 100)
            .WithMessage("The API key name must be between 1 and 100 characters and cannot be whitespace-only.");

        RuleFor(command => command.ExpirationDays)
            .InclusiveBetween(1, 365)
            .WithMessage("The expiration period must be between 1 and 365 days.");
    }
}
