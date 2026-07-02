using Aethria.Api.Endpoints.Auth;
using Aethria.Application.UseCases.Auth.ChangePassword;
using Aethria.Application.UseCases.Auth.LoginOrRegisterWithGoogle;
using Aethria.Application.UseCases.Auth.LoginWithEmail;
using Aethria.Application.UseCases.Auth.Logout;
using Aethria.Application.UseCases.Auth.RefreshAccessToken;
using Aethria.Application.UseCases.Auth.Register;
using AuthUseCaseResponse = Aethria.Application.UseCases.Auth.AuthenticationResult;

namespace Aethria.Api.Endpoints;

internal static class AuthEndpoints
{
    internal static void MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/auth")
            .WithTags("Auth");

        group.MapPost("register", Register)
            .AllowAnonymous()
            .WithName("Register")
            .Produces<AuthResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest);

        group.MapPost("login", Login)
            .AllowAnonymous()
            .WithName("Login")
            .Produces<AuthResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized);

        group.MapPost("google", Google)
            .AllowAnonymous()
            .WithName("LoginOrRegisterWithGoogle")
            .Produces<AuthResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized);

        group.MapPost("refresh", Refresh)
            .AllowAnonymous()
            .WithName("RefreshAccessToken")
            .Produces<AuthResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized);

        group.MapPost("logout", Logout)
            .AllowAnonymous()
            .WithName("Logout")
            .Produces(StatusCodes.Status204NoContent);

        group.MapPost("change-password", ChangePassword)
            .RequireAuthorization()
            .WithName("ChangePassword")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized);
    }

    /// <summary>
    /// Register a user with email and password.
    /// </summary>
    public static async Task<IResult> Register(
        [FromBody] RegisterRequest request,
        [FromServices] IMediator mediator,
        [FromServices] IConfiguration configuration,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new RegisterCommand(request.Email, request.Password),
            cancellationToken);
        return ToAuthResult(result, configuration, httpContext);
    }

    /// <summary>
    /// Login with email and password.
    /// </summary>
    public static async Task<IResult> Login(
        [FromBody] LoginRequest request,
        [FromServices] IMediator mediator,
        [FromServices] IConfiguration configuration,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new LoginWithEmailCommand(request.Email, request.Password),
            cancellationToken);
        return ToAuthResult(result, configuration, httpContext);
    }

    /// <summary>
    /// Login or register using a Google ID token.
    /// </summary>
    public static async Task<IResult> Google(
        [FromBody] GoogleRequest request,
        [FromServices] IMediator mediator,
        [FromServices] IConfiguration configuration,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new LoginOrRegisterWithGoogleCommand(request.IdToken),
            cancellationToken);
        return ToAuthResult(result, configuration, httpContext);
    }

    /// <summary>
    /// Validate the refresh token cookie and issue a new access token.
    /// </summary>
    public static async Task<IResult> Refresh(
        [FromServices] IMediator mediator,
        [FromServices] IConfiguration configuration,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var cookieName = GetRefreshTokenCookieName(configuration);
        if (!httpContext.Request.Cookies.TryGetValue(cookieName, out var refreshToken)
            || string.IsNullOrWhiteSpace(refreshToken))
        {
            return Results.Unauthorized();
        }

        var result = await mediator.Send(
            new RefreshAccessTokenCommand(refreshToken),
            cancellationToken);
        return ToAuthResult(result, configuration, httpContext);
    }

    /// <summary>
    /// Revoke the current refresh token and clear the refresh-token cookie.
    /// </summary>
    public static async Task<IResult> Logout(
        [FromServices] IMediator mediator,
        [FromServices] IConfiguration configuration,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var cookieName = GetRefreshTokenCookieName(configuration);
        httpContext.Request.Cookies.TryGetValue(cookieName, out var refreshToken);

        var result = await mediator.Send(
            new LogoutCommand(refreshToken),
            cancellationToken);
        if (result.IsFailed)
        {
            return result.ToErrorResult();
        }

        ClearRefreshTokenCookie(cookieName, httpContext.Response);
        return Results.NoContent();
    }

    /// <summary>
    /// Change the authenticated user's password.
    /// </summary>
    public static async Task<IResult> ChangePassword(
        [FromBody] ChangePasswordRequest request,
        [FromServices] IMediator mediator,
        ClaimsPrincipal user,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new ChangePasswordCommand(
                user.GetUserId(),
                request.CurrentPassword,
                request.NewPassword),
            cancellationToken);

        if (result.IsFailed)
        {
            return result.ToErrorResult();
        }

        return Results.NoContent();
    }

    private static IResult ToAuthResult(
        FluentResults.Result<AuthUseCaseResponse> result,
        IConfiguration configuration,
        HttpContext httpContext)
    {
        if (result.IsFailed)
        {
            return result.ToErrorResult();
        }

        var cookieName = GetRefreshTokenCookieName(configuration);
        SetRefreshTokenCookie(cookieName, result.Value.RefreshToken, result.Value.RefreshTokenExpiresAt, httpContext.Response);

        return Results.Ok(new AuthResponse
        {
            UserId = result.Value.UserId,
            Email = result.Value.Email,
            AccessToken = result.Value.AccessToken,
            AccessTokenExpiresAt = result.Value.AccessTokenExpiresAt
        });
    }

    private static string GetRefreshTokenCookieName(IConfiguration configuration)
    {
        return configuration["Auth:RefreshTokenCookieName"]
            ?? throw new InvalidOperationException("Missing required configuration value 'Auth:RefreshTokenCookieName'.");
    }

    private static void SetRefreshTokenCookie(string cookieName, string refreshToken, DateTimeOffset expiresAt, HttpResponse response)
    {
        response.Cookies.Append(
            cookieName,
            refreshToken,
            CreateRefreshCookieOptions(expiresAt));
    }

    private static void ClearRefreshTokenCookie(string cookieName, HttpResponse response)
    {
        response.Cookies.Delete(
            cookieName,
            CreateRefreshCookieOptions(DateTimeOffset.UtcNow.AddDays(-1)));
    }

    private static CookieOptions CreateRefreshCookieOptions(DateTimeOffset expiresAt)
    {
        return new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.None,
            Path = "/api/auth",
            Expires = expiresAt
        };
    }
}
