namespace Aethria.Application.Abstractions.Identity;

public sealed record IdentityAuthOperationResult(
    bool Succeeded,
    AuthUser? User,
    IReadOnlyList<string> Errors)
{
    public static IdentityAuthOperationResult Success(AuthUser user)
    {
        return new IdentityAuthOperationResult(true, user, []);
    }

    public static IdentityAuthOperationResult Failed(IEnumerable<string> errors)
    {
        return new IdentityAuthOperationResult(false, null, [.. errors]);
    }
}
