using Aethria.Api.Authentication;
using Aethria.Api.OpenApi;
using Aethria.Api.Realtime;
using Aethria.Application.UseCases.Notifications;
using Microsoft.AspNetCore.Authentication;

namespace Aethria.Api;

internal static class DependencyInjection
{
    internal static IServiceCollection AddApiServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSignalR();
        services.AddScoped<INotificationEventPublisher, NotificationEventPublisher>();

        services.AddOpenApi(options =>
        {
            options.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
            options.AddOperationTransformer<RequireBearerAuthorizationOperationTransformer>();
        });

        services.AddValidation();
        services.AddAuthentication(JwtAccessTokenAuthenticationHandler.SchemeName)
            .AddScheme<AuthenticationSchemeOptions, JwtAccessTokenAuthenticationHandler>(
                JwtAccessTokenAuthenticationHandler.SchemeName,
                options => { });
        services.AddAuthorization();

        services.AddConfigurableCors(configuration);

        return services;
    }
}
