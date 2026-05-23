namespace Aethria.Application.UseCases.Notifications.MarkNotificationAsRead;

internal sealed class MarkNotificationAsReadCommandValidator : AbstractValidator<MarkNotificationAsReadCommand>
{
    public MarkNotificationAsReadCommandValidator()
    {
        RuleFor(command => command.UserId)
            .NotEmpty()
            .WithMessage("UserId is required.");

        RuleFor(command => command.NotificationId)
            .NotEmpty()
            .WithMessage("NotificationId is required.");
    }
}

