namespace Aethria.Api.Endpoints.Notifications;

/// <summary>
/// Query parameters for getting paginated notifications.
/// </summary>
public sealed record GetPageNotificationsRequest
{
    /// <summary>
    /// Page number to retrieve.
    /// </summary>
    [Range(1, int.MaxValue, ErrorMessage = "Page number must be greater than or equal to 1.")]
    public int PageNumber { get; init; } = 1;

    /// <summary>
    /// Number of items per page.
    /// </summary>
    [Range(1, 100, ErrorMessage = "Page size must be between 1 and 100.")]
    public int PageSize { get; init; } = 10;

    /// <summary>
    /// Optional filter for read/unread notifications.
    /// </summary>
    public bool? IsRead { get; init; }
}

