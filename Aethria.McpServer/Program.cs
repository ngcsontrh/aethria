using Aethria.Application;
using Aethria.Infrastructure;
using Aethria.Infrastructure.AgentFramework;
using Aethria.McpServer.Tools;
using ModelContextProtocol.AspNetCore.Authentication;
using ModelContextProtocol.Authentication;

var builder = WebApplication.CreateBuilder(args);
const string McpEndpointPattern = "/mcp";

builder.AddServiceDefaults();
builder.AddOpenTelemetrySources(
    metrics => metrics.AddMeter(
        AgentFrameworkTelemetry.SourceName,
        AgentFrameworkTelemetry.WorkflowsSourceName),
    tracing => tracing.AddSource(
        AgentFrameworkTelemetry.SourceName,
        AgentFrameworkTelemetry.WorkflowsSourceName));

builder.Services.AddMcpInfrastructureServices(builder.Configuration);

builder.Services.AddMcpApplicationServices();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = ApiKeyAuthenticationHandler.SchemeName;
    options.DefaultChallengeScheme = McpAuthenticationDefaults.AuthenticationScheme;
})
.AddScheme<AuthenticationSchemeOptions, ApiKeyAuthenticationHandler>(
    ApiKeyAuthenticationHandler.SchemeName,
    options => { })
.AddMcp(options =>
{
    options.ResourceMetadata = new ProtectedResourceMetadata
    {
        ResourceName = "Aethria MCP",
        BearerMethodsSupported = ["header"]
    };
});
builder.Services.AddAuthorization();

builder.Services.AddMcpServer()
    .WithHttpTransport(options =>
    {
        options.Stateless = true;
    })
    .AddAuthorizationFilters()
    .WithTools<ResourceTool>();

var app = builder.Build();
app.MapDefaultEndpoints();

app.UseAuthentication();
app.UseAuthorization();

app.MapMcp(McpEndpointPattern).RequireAuthorization();

app.Run();
