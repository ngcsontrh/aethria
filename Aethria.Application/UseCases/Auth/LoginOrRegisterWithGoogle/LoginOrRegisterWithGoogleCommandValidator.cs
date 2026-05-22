namespace Aethria.Application.UseCases.Auth.LoginOrRegisterWithGoogle;

internal sealed class LoginOrRegisterWithGoogleCommandValidator : AbstractValidator<LoginOrRegisterWithGoogleCommand>
{
    public LoginOrRegisterWithGoogleCommandValidator()
    {
        RuleFor(command => command.IdToken)
            .NotEmpty()
            .WithMessage("Google token is required.");
    }
}
