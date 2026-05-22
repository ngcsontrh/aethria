namespace Aethria.Application.Abstractions.Identity;

public sealed record ExternalLoginInfo(
    string LoginProvider,
    string ProviderKey,
    string ProviderDisplayName);
