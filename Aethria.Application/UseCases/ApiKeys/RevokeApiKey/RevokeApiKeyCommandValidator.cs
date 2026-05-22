namespace Aethria.Application.UseCases.ApiKeys.RevokeApiKey;

internal sealed class RevokeApiKeyCommandValidator : AbstractValidator<RevokeApiKeyCommand>
{
    public RevokeApiKeyCommandValidator()
    {
        RuleFor(command => command.UserId)
            .NotEmpty()
            .WithMessage("UserId is required.");

        RuleFor(command => command.KeyId)
            .NotEmpty()
            .WithMessage("KeyId is required.");
    }
}
