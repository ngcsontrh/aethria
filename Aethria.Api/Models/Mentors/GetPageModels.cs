namespace Aethria.Api.Endpoints.Mentors;

/// <summary>
/// Query parameters for getting paginated mentors.
/// </summary>
public sealed record GetPageMentorsRequest
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
}
