namespace Aethria.Application.Abstractions.Identity;

public interface IIdentityAuthService
{
    Task<AuthUser?> FindByIdAsync(Guid userId, CancellationToken cancellationToken);

    Task<AuthUser?> FindByEmailAsync(string email, CancellationToken cancellationToken);

    Task<AuthUser?> FindByLoginAsync(string loginProvider, string providerKey, CancellationToken cancellationToken);

    Task<IdentityAuthOperationResult> CreateUserAsync(string email, string? password, bool emailConfirmed, CancellationToken cancellationToken);

    Task<bool> CheckPasswordAsync(Guid userId, string password, CancellationToken cancellationToken);

    Task<IdentityAuthOperationResult> ChangePasswordAsync(Guid userId, string currentPassword, string newPassword, CancellationToken cancellationToken);

    Task<IdentityAuthOperationResult> AddLoginAsync(Guid userId, ExternalLoginInfo loginInfo, CancellationToken cancellationToken);
}
