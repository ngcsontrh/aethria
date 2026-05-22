using Aethria.Application.Abstractions.Identity;
using Microsoft.AspNetCore.Identity;

namespace Aethria.Infrastructure.Identity;

internal sealed class IdentityAuthService : IIdentityAuthService
{
    private readonly UserManager<AppUser> _userManager;

    public IdentityAuthService(UserManager<AppUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<AuthUser?> FindByIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        return user is null ? null : new AuthUser(user.Id, user.Email ?? string.Empty);
    }

    public async Task<AuthUser?> FindByEmailAsync(string email, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByEmailAsync(email);
        return user is null ? null : new AuthUser(user.Id, user.Email ?? string.Empty);
    }

    public async Task<AuthUser?> FindByLoginAsync(string loginProvider, string providerKey, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByLoginAsync(loginProvider, providerKey);
        return user is null ? null : new AuthUser(user.Id, user.Email ?? string.Empty);
    }

    public async Task<IdentityAuthOperationResult> CreateUserAsync(string email, string? password, bool emailConfirmed, CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        var user = new AppUser
        {
            Id = Guid.NewGuid(),
            UserName = email,
            Email = email,
            EmailConfirmed = emailConfirmed,
            CreatedAt = now,
            UpdatedAt = now
        };

        var result = password is null
            ? await _userManager.CreateAsync(user)
            : await _userManager.CreateAsync(user, password);

        return result.Succeeded
            ? IdentityAuthOperationResult.Success(new AuthUser(user.Id, user.Email ?? user.UserName ?? string.Empty))
            : IdentityAuthOperationResult.Failed(result.Errors.Select(error => error.Description));
    }

    public async Task<bool> CheckPasswordAsync(Guid userId, string password, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        return user is not null && await _userManager.CheckPasswordAsync(user, password);
    }

    public async Task<IdentityAuthOperationResult> ChangePasswordAsync(Guid userId, string currentPassword, string newPassword, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null)
        {
            return IdentityAuthOperationResult.Failed(["User was not found."]);
        }

        var result = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);
        if (!result.Succeeded)
        {
            return IdentityAuthOperationResult.Failed(result.Errors.Select(error => error.Description));
        }

        user.UpdatedAt = DateTimeOffset.UtcNow;
        await _userManager.UpdateAsync(user);

        return IdentityAuthOperationResult.Success(new AuthUser(user.Id, user.Email ?? user.UserName ?? string.Empty));
    }

    public async Task<IdentityAuthOperationResult> AddLoginAsync(
        Guid userId,
        Aethria.Application.Abstractions.Identity.ExternalLoginInfo loginInfo,
        CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null)
        {
            return IdentityAuthOperationResult.Failed(["User was not found."]);
        }

        var result = await _userManager.AddLoginAsync(
            user,
            new UserLoginInfo(loginInfo.LoginProvider, loginInfo.ProviderKey, loginInfo.ProviderDisplayName));

        return result.Succeeded
            ? IdentityAuthOperationResult.Success(new AuthUser(user.Id, user.Email ?? string.Empty))
            : IdentityAuthOperationResult.Failed(result.Errors.Select(error => error.Description));
    }

}
