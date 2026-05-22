namespace Aethria.Application.UseCases.Chat.Contracts;

public sealed record ChatStreamResponse(
    string Status,
    string? Delta = null,
    string? Answer = null,
    Guid? SessionId = null,
    string? Message = null)
{
    public static class Statuses
    {
        public const string Started = "started";
        public const string Delta = "delta";
        public const string Completed = "completed";
        public const string Failed = "failed";
    }
}
