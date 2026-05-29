using Aethria.Api.Authentication;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace Aethria.Api.OpenApi;

internal sealed class RequireBearerAuthorizationOperationTransformer : IOpenApiOperationTransformer
{
    public Task TransformAsync(
        OpenApiOperation operation,
        OpenApiOperationTransformerContext context,
        CancellationToken cancellationToken)
    {
        var endpointMetadata = context.Description.ActionDescriptor.EndpointMetadata;
        var hasAllowAnonymous = endpointMetadata.OfType<IAllowAnonymous>().Any();
        if (hasAllowAnonymous)
        {
            return Task.CompletedTask;
        }

        var hasAuthorization = endpointMetadata.OfType<IAuthorizeData>().Any();
        if (!hasAuthorization)
        {
            return Task.CompletedTask;
        }

        operation.Security ??= new List<OpenApiSecurityRequirement>();
        operation.Security.Add(new OpenApiSecurityRequirement
        {
            [new OpenApiSecuritySchemeReference(JwtAccessTokenAuthenticationHandler.SchemeName, context.Document)] = new List<string>()
        });

        return Task.CompletedTask;
    }
}
