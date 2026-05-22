using Aethria.Application.ErrorTypes;

namespace Aethria.Api.Extensions;

/// <summary>
/// Result mapping helpers.
/// </summary>
internal static class ResultExtensions
{
    /// <summary>
    /// Maps a failed Result to the appropriate IResult based on the error type.
    /// </summary>
    public static IResult ToErrorResult(this ResultBase result)
    {
        if (result.HasError<NotFoundError>())
        {
            return ToErrorResult(result.Errors.OfType<NotFoundError>(), StatusCodes.Status404NotFound);
        }

        if (result.HasError<ValidationError>())
        {
            return ToErrorResult(result.Errors.OfType<ValidationError>(), StatusCodes.Status400BadRequest);
        }

        if (result.HasError<ConflictError>())
        {
            return ToErrorResult(result.Errors.OfType<ConflictError>(), StatusCodes.Status409Conflict);
        }

        if (result.HasError<UnauthorizedError>())
        {
            return ToErrorResult(result.Errors.OfType<UnauthorizedError>(), StatusCodes.Status401Unauthorized);
        }

        if (result.HasError<TimeoutError>())
        {
            return ToErrorResult(result.Errors.OfType<TimeoutError>(), StatusCodes.Status504GatewayTimeout);
        }

        if (result.HasError<ServiceUnavailableError>())
        {
            return ToErrorResult(result.Errors.OfType<ServiceUnavailableError>(), StatusCodes.Status503ServiceUnavailable);
        }

        if (result.HasError<InternalError>())
        {
            return ToErrorResult(result.Errors.OfType<InternalError>(), StatusCodes.Status500InternalServerError);
        }

        return ToErrorResult(result.Errors, StatusCodes.Status500InternalServerError);
    }

    private static IResult ToErrorResult(IEnumerable<IError> errors, int statusCode)
    {
        return Results.Json(new { error = ToErrorMessage(errors) }, statusCode: statusCode);
    }

    private static string ToErrorMessage(IEnumerable<IError> errors)
    {
        var errorMessage = string.Join("; ", errors.Select(e => e.Message).Where(message => !string.IsNullOrWhiteSpace(message)));
        return string.IsNullOrWhiteSpace(errorMessage) ? "An unexpected error occurred." : errorMessage;
    }
}
