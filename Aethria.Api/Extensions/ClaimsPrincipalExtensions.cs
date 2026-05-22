namespace Aethria.Api.Extensions;

/// <summary>
/// Claims principal helpers.
/// </summary>
internal static class ClaimsPrincipalExtensions
{
    /// <summary>
    /// Gets the authenticated user's identifier from the name identifier claim.
    /// </summary>
    public static Guid GetUserId(this ClaimsPrincipal user)
    {
        var userIdString = user.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(userIdString, out var userId) ? userId : Guid.Empty;
    }
}
