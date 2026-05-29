using Aethria.Infrastructure.AgentFramework.Chat;
using Aethria.Infrastructure.AgentFramework.Mentors;
using Aethria.Infrastructure.AgentFramework.Quiz;
using Aethria.Infrastructure.AgentFramework.Roadmap;
using Aethria.Infrastructure.Chunking;
using Aethria.Infrastructure.DomainEvents;
using Aethria.Infrastructure.Embedding;
using Aethria.Infrastructure.Identity;
using Aethria.Infrastructure.Repositories;
using Aethria.Infrastructure.Storage;
using Aethria.Infrastructure.VectorSearch;
using Azure.Storage.Blobs;
using Cohere;
using Tavily;

namespace Aethria.Infrastructure;

public static class DependencyInjection
{
    /// <summary>
    /// Registers all infrastructure services. Use this for projects that need the full stack.
    /// </summary>
    public static IServiceCollection AddApiInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");

        services.AddDbContext<AppDbContext>(opts =>
        {
            opts.UseNpgsql(
                connectionString,
                o =>
                {
                    o.UseVector();
                    o.EnableRetryOnFailure(
                        maxRetryCount: 5,
                        maxRetryDelay: TimeSpan.FromSeconds(10),
                        errorCodesToAdd: null);
                    o.CommandTimeout(60);
                });
        });

        services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();
        services.AddScoped<IUnitOfWork, UnitOfWork.UnitOfWork>();

        services.AddScoped<IChatSessionRepository, ChatSessionRepository>();
        services.AddScoped<IMentorRepository, MentorRepository>();
        services.AddScoped<INotificationRepository, NotificationRepository>();
        services.AddScoped<IResourceRepository, ResourceRepository>();
        services.AddScoped<IRoadmapRepository, RoadmapRepository>();
        services.AddScoped<IQuizRepository, QuizRepository>();
        services.AddScoped<IApiKeyRepository, ApiKeyRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();

        services.AddOptions<AuthOptions>()
            .Bind(configuration.GetSection(AuthOptions.SectionName))
            .Validate(
                options => !string.IsNullOrWhiteSpace(options.Issuer),
                "Auth:Issuer is required.")
            .Validate(
                options => !string.IsNullOrWhiteSpace(options.Audience),
                "Auth:Audience is required.")
            .Validate(
                options => !string.IsNullOrWhiteSpace(options.SigningKey),
                "Auth:SigningKey is required.")
            .Validate(
                options => options.AccessTokenMinutes > 0,
                "Auth:AccessTokenMinutes must be greater than 0.")
            .Validate(
                options => options.RefreshTokenDays > 0,
                "Auth:RefreshTokenDays must be greater than 0.")
            .Validate(
                options => !string.IsNullOrWhiteSpace(options.GoogleClientId),
                "Auth:GoogleClientId is required.");

        services.AddIdentityCore<AppUser>(options =>
        {
            options.User.RequireUniqueEmail = true;
            options.Password.RequiredLength = 8;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequireUppercase = false;
            options.Password.RequireDigit = false;
        })
            .AddRoles<AppRole>()
            .AddEntityFrameworkStores<AppDbContext>();

        services.AddScoped<IIdentityAuthService, IdentityAuthService>();
        services.AddScoped<IGoogleTokenValidator, GoogleTokenValidator>();
        services.AddScoped<IAuthTokenService, AuthTokenService>();
        services.AddScoped<IApiKeyTokenService, ApiKeyTokenService>();

        services.AddOptions<AzureStorageOptions>()
            .Bind(configuration.GetSection(AzureStorageOptions.SectionName))
            .Validate(
                options => !string.IsNullOrWhiteSpace(options.ConnectionString),
                "AzureStorage:ConnectionString is required.");

        services.AddSingleton(sp =>
        {
            var options = sp.GetRequiredService<IOptions<AzureStorageOptions>>().Value;
            return new BlobServiceClient(options.ConnectionString);
        });

        services.AddScoped<IFileStorageService, AzureBlobStorageService>();

        services.AddOptions<FoundryOptions>()
            .Bind(configuration.GetSection(FoundryOptions.SectionName))
            .Validate(
                options => !string.IsNullOrWhiteSpace(options.ProjectEndpoint),
                "Foundry:ProjectEndpoint is required.")
            .Validate(
                options => Uri.TryCreate(options.ProjectEndpoint, UriKind.Absolute, out _),
                "Foundry:ProjectEndpoint must be an absolute URI.")
            .Validate(
                options => !string.IsNullOrWhiteSpace(options.AzureOpenAIEndPoint),
                "Foundry:AzureOpenAIEndPoint is required.")
            .Validate(
                options => Uri.TryCreate(options.AzureOpenAIEndPoint, UriKind.Absolute, out _),
                "Foundry:AzureOpenAIEndPoint must be an absolute URI.")
            .Validate(
                options => !string.IsNullOrWhiteSpace(options.ApiKey),
                "Foundry:ApiKey is required.");

        services.AddSingleton<IEmbeddingService, AzureOpenAIEmbeddingService>();

        services.AddSingleton(sp =>
        {
            var options = sp.GetRequiredService<IOptions<FoundryOptions>>().Value;
            var endpoint = new Uri(options.ProjectEndpoint);
            var cohereBaseUri = endpoint.AbsolutePath.Contains("/providers/cohere", StringComparison.OrdinalIgnoreCase)
                ? endpoint
                : new Uri($"{endpoint.GetLeftPart(UriPartial.Authority)}/providers/cohere");

            return new CohereClient(
                httpClient: null,
                baseUri: cohereBaseUri,
                authorizations:
                [
                    new Cohere.EndPointAuthorization
                    {
                        Type = "Http",
                        SchemeId = "BearerAuth",
                        Location = "Header",
                        Name = "Bearer",
                        Value = options.ApiKey,
                    }
                ],
                disposeHttpClient: true);
        });

        // services.AddOptions<QdrantOptions>()
        //     .Bind(configuration.GetSection(QdrantOptions.SectionName))
        //     .Validate(
        //         options => !string.IsNullOrWhiteSpace(options.Endpoint),
        //         "Qdrant:Endpoint is required.")
        //     .Validate(
        //         options => Uri.TryCreate(options.Endpoint, UriKind.Absolute, out _),
        //         "Qdrant:Endpoint must be an absolute URI.")
        //     .Validate(
        //         options => !string.IsNullOrWhiteSpace(options.ApiKey),
        //         "Qdrant:ApiKey is required.");

        services.AddScoped<IResourceChunkVectorStore, PgvectorResourceChunkVectorStore>();

        services.AddSingleton<ITokenCountingService, OpenAITokenCountingService>();
        services.AddSingleton<ITextChunkingService, OpenAITextChunkingService>();

        services.AddOptions<TavilyOptions>()
            .Bind(configuration.GetSection(TavilyOptions.SectionName))
            .Validate(
                options => !string.IsNullOrWhiteSpace(options.ApiKey),
                "Tavily:ApiKey is required.");

        services.AddSingleton(sp =>
        {
            var options = sp.GetRequiredService<IOptions<TavilyOptions>>().Value;
            return new TavilyClient(options.ApiKey);
        });

        services.AddKeyedScoped<IChatAgent, ResourceChatAgent>("resource-chat");
        services.AddKeyedScoped<IChatAgent, GeneralChatAgent>("general-chat");
        services.AddKeyedScoped<IChatAgent, MentorChatAgent>("mentor-chat");
        services.AddScoped<IMentorValidatorAgent, MentorValidatorAgent>();
        services.AddScoped<IAIQuizGenerationWorkflow, QuizAgentWorkflow>();
        services.AddScoped<IAIRoadmapGenerationAgent, AIRoadmapGenerationAgent>();

        return services;
    }

    /// <summary>
    /// Registers only the infrastructure services required by the MCP server.
    /// </summary>
    public static IServiceCollection AddMcpInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");

        services.AddDbContext<AppDbContext>(opts =>
        {
            opts.UseNpgsql(
                connectionString,
                o =>
                {
                    o.UseVector();
                    o.EnableRetryOnFailure(
                        maxRetryCount: 5,
                        maxRetryDelay: TimeSpan.FromSeconds(10),
                        errorCodesToAdd: null);
                    o.CommandTimeout(60);
                });
        });

        services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();
        services.AddScoped<IUnitOfWork, UnitOfWork.UnitOfWork>();
        services.AddScoped<IApiKeyRepository, ApiKeyRepository>();
        services.AddScoped<IResourceRepository, ResourceRepository>();
        services.AddScoped<IChatSessionRepository, ChatSessionRepository>();

        services.AddOptions<FoundryOptions>()
            .Bind(configuration.GetSection(FoundryOptions.SectionName))
            .Validate(
                options => !string.IsNullOrWhiteSpace(options.ProjectEndpoint),
                "Foundry:ProjectEndpoint is required.")
            .Validate(
                options => Uri.TryCreate(options.ProjectEndpoint, UriKind.Absolute, out _),
                "Foundry:ProjectEndpoint must be an absolute URI.")
            .Validate(
                options => !string.IsNullOrWhiteSpace(options.AzureOpenAIEndPoint),
                "Foundry:AzureOpenAIEndPoint is required.")
            .Validate(
                options => Uri.TryCreate(options.AzureOpenAIEndPoint, UriKind.Absolute, out _),
                "Foundry:AzureOpenAIEndPoint must be an absolute URI.")
            .Validate(
                options => !string.IsNullOrWhiteSpace(options.ApiKey),
                "Foundry:ApiKey is required.");

        services.AddSingleton<IEmbeddingService, AzureOpenAIEmbeddingService>();

        services.AddSingleton(sp =>
        {
            var options = sp.GetRequiredService<IOptions<FoundryOptions>>().Value;
            var endpoint = new Uri(options.ProjectEndpoint);
            var cohereBaseUri = endpoint.AbsolutePath.Contains("/providers/cohere", StringComparison.OrdinalIgnoreCase)
                ? endpoint
                : new Uri($"{endpoint.GetLeftPart(UriPartial.Authority)}/providers/cohere");

            return new CohereClient(
                httpClient: null,
                baseUri: cohereBaseUri,
                authorizations:
                [
                    new Cohere.EndPointAuthorization
                    {
                        Type = "Http",
                        SchemeId = "BearerAuth",
                        Location = "Header",
                        Name = "Bearer",
                        Value = options.ApiKey,
                    }
                ],
                disposeHttpClient: true);
        });

        // services.AddOptions<QdrantOptions>()
        //     .Bind(configuration.GetSection(QdrantOptions.SectionName))
        //     .Validate(
        //         options => !string.IsNullOrWhiteSpace(options.Endpoint),
        //         "Qdrant:Endpoint is required.")
        //     .Validate(
        //         options => Uri.TryCreate(options.Endpoint, UriKind.Absolute, out _),
        //         "Qdrant:Endpoint must be an absolute URI.")
        //     .Validate(
        //         options => !string.IsNullOrWhiteSpace(options.ApiKey),
        //         "Qdrant:ApiKey is required.");

        services.AddScoped<IResourceChunkVectorStore, PgvectorResourceChunkVectorStore>();
        services.AddKeyedScoped<IChatAgent, ResourceChatAgent>("resource-chat");

        return services;
    }
}
