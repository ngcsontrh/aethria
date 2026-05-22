using Aethria.Application.Abstractions.Identity;
using Google.Apis.Auth;

namespace Aethria.Infrastructure.Identity;

internal sealed class GoogleTokenValidator : IGoogleTokenValidator
{
    private readonly AuthOptions _options;

    public GoogleTokenValidator(IOptions<AuthOptions> options)
    {
        _options = options.Value;
    }

    public async Task<GoogleUserInfo?> ValidateAsync(string idToken, CancellationToken cancellationToken)
    {
        var settings = new GoogleJsonWebSignature.ValidationSettings();
        settings.Audience = [_options.GoogleClientId];

        var payload = await GoogleJsonWebSignature.ValidateAsync(idToken, settings);

        return new GoogleUserInfo(
            Subject: payload.Subject,
            Email: payload.Email,
            EmailVerified: payload.EmailVerified);
    }
}
