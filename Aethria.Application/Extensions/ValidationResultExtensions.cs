using FluentValidation.Results;

namespace Aethria.Application.Extensions;

internal static class ValidationResultExtensions
{
    public static ValidationError ToValidationError(this ValidationResult validationResult)
    {
        var message = string.Join(
            "; ",
            validationResult.Errors
                .Select(error => error.ErrorMessage)
                .Where(message => !string.IsNullOrWhiteSpace(message))
                .Distinct());

        return ValidationError.WithMessage(
            string.IsNullOrWhiteSpace(message)
                ? "Validation failed."
                : message);
    }
}
