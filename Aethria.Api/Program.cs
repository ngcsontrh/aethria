using Aethria.Api;
using Aethria.Api.Authentication;
using Aethria.Api.Endpoints;
using Aethria.Api.Hubs;
using Aethria.Application;
using Aethria.Infrastructure;
using Scalar.AspNetCore;

const string AgentFrameworkSourceName = "Experimental.Microsoft.Agents.AI";
const string AgentFrameworkWorkflowsSourceName = "Microsoft.Agents.AI.Workflows";

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.AddOpenTelemetrySources(
    metrics => metrics.AddMeter(
        AgentFrameworkSourceName,
        AgentFrameworkWorkflowsSourceName),
    tracing => tracing.AddSource(
        AgentFrameworkSourceName,
        AgentFrameworkWorkflowsSourceName));

builder.Services.AddApiInfrastructureServices(builder.Configuration);
builder.Services.AddApiApplicationServices();
builder.Services.AddApiServices(builder.Configuration);

var app = builder.Build();

app.MapDefaultEndpoints();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options.AddPreferredSecuritySchemes(JwtAccessTokenAuthenticationHandler.SchemeName);
    });
}

app.UseCors();

app.UseAuthentication();
app.UseAuthorization();

app.MapApiKeyEndpoints();
app.MapAuthEndpoints();
app.MapChatEndpoints();
app.MapChatSessionEndpoints();
app.MapMentorEndpoints();
app.MapNotificationEndpoints();
app.MapQuizEndpoints();
app.MapResourceEndpoints();
app.MapRoadmapEndpoints();

app.MapHub<ChatHub>("/hubs/chat");
app.MapHub<NotificationHub>("/hubs/notifications");

app.Run();
