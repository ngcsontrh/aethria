namespace Aethria.Api.Extensions;

internal static class StreamResultExtensions
{
    internal static string ToStreamFailureMessage(this ResultBase result, string fallbackMessage)
    {
        var errorMessage = result.Errors.FirstOrDefault()?.Message;
        return string.IsNullOrWhiteSpace(errorMessage) ? fallbackMessage : errorMessage;
    }
}
