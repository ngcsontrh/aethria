using Aethria.Api.Authentication;
using Aethria.Api.Endpoints;
using Aethria.Api.Hubs;
using Aethria.Application;
using Aethria.Infrastructure;
using Microsoft.AspNetCore.Authentication;
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
builder.Services.AddSignalR();
builder.Services.AddOpenApi();
builder.Services.AddValidation();
builder.Services.AddAuthentication(JwtAccessTokenAuthenticationHandler.SchemeName)
    .AddScheme<AuthenticationSchemeOptions, JwtAccessTokenAuthenticationHandler>(
        JwtAccessTokenAuthenticationHandler.SchemeName,
        options => { });
builder.Services.AddAuthorization();

builder.Services.AddConfigurableCors(builder.Configuration);

var app = builder.Build();

app.MapDefaultEndpoints();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseCors();

app.UseAuthentication();
app.UseAuthorization();

app.MapApiKeyEndpoints();
app.MapAuthEndpoints();
app.MapChatEndpoints();
app.MapChatSessionEndpoints();
app.MapMentorEndpoints();
app.MapQuizEndpoints();
app.MapResourceEndpoints();
app.MapRoadmapEndpoints();

app.MapHub<ChatHub>("/hubs/chat");
app.MapHub<QuizHub>("/hubs/quizzes");
app.MapHub<RoadmapHub>("/hubs/roadmaps");

app.Run();
