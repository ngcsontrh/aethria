using Aethria.Application.ErrorTypes;

namespace Aethria.McpServer.Extensions;

/// <summary>
/// Result mapping helpers.
/// </summary>
internal static class ResultExtensions
{
    /// <summary>
    /// Maps a Result to the standard MCP result envelope.
    /// </summary>
    public static McpResult ToMcpResult<T>(this Result<T> result)
    {
        return result.IsSuccess
            ? McpResult.Success(result.Value)
            : result.ToMcpErrorResult();
    }

    /// <summary>
    /// Maps a failed Result to the standard MCP error envelope based on the error type.
    /// </summary>
    public static McpResult ToMcpErrorResult(this ResultBase result)
    {
        if (result.HasError<NotFoundError>())
        {
            return ToMcpErrorResult(result.Errors.OfType<NotFoundError>());
        }

        if (result.HasError<ValidationError>())
        {
            return ToMcpErrorResult(result.Errors.OfType<ValidationError>());
        }

        if (result.HasError<ConflictError>())
        {
            return ToMcpErrorResult(result.Errors.OfType<ConflictError>());
        }

        if (result.HasError<UnauthorizedError>())
        {
            return ToMcpErrorResult(result.Errors.OfType<UnauthorizedError>());
        }

        if (result.HasError<TimeoutError>())
        {
            return ToMcpErrorResult(result.Errors.OfType<TimeoutError>());
        }

        if (result.HasError<ServiceUnavailableError>())
        {
            return ToMcpErrorResult(result.Errors.OfType<ServiceUnavailableError>());
        }

        if (result.HasError<InternalError>())
        {
            return ToMcpErrorResult(result.Errors.OfType<InternalError>());
        }

        return ToMcpErrorResult(result.Errors);
    }

    private static McpResult ToMcpErrorResult(IEnumerable<IError> errors)
    {
        var errorMessage = string.Join("; ", errors.Select(e => e.Message).Where(message => !string.IsNullOrWhiteSpace(message)));
        return McpResult.Error(string.IsNullOrWhiteSpace(errorMessage) ? "An unexpected error occurred." : errorMessage);
    }
}
