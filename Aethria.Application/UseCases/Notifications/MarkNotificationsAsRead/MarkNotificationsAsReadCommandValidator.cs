namespace Aethria.Application.UseCases.Notifications.MarkNotificationsAsRead;

internal sealed class MarkNotificationsAsReadCommandValidator : AbstractValidator<MarkNotificationsAsReadCommand>
{
    public MarkNotificationsAsReadCommandValidator()
    {
        RuleFor(command => command.UserId)
            .NotEmpty()
            .WithMessage("UserId is required.");

        RuleFor(command => command.NotificationIds)
            .NotNull()
            .WithMessage("NotificationIds is required.");

        RuleForEach(command => command.NotificationIds)
            .NotEmpty()
            .WithMessage("NotificationId is required.");
    }
}

