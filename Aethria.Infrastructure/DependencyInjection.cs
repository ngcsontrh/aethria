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
        services.AddInfrastructurePersistence(configuration);
        services.AddIdentityInfrastructure(configuration);
        services.AddStorageInfrastructure(configuration);
        services.AddFullAiInfrastructure(configuration);

        return services;
    }

    /// <summary>
    /// Registers only the infrastructure services required by the MCP server.
    /// </summary>
    public static IServiceCollection AddMcpInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAppDbContext(configuration);
        services.AddUnitOfWorkAndDomainEvents();
        services.AddApiKeyPersistence();
        services.AddResourcePersistence();
        services.AddChatSessionPersistence();
        services.AddResourceChatAiInfrastructure(configuration);

        return services;
    }

    /// <summary>
    /// Registers persistence services: DbContext, repositories, UnitOfWork, and domain events.
    /// </summary>
    public static IServiceCollection AddInfrastructurePersistence(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAppDbContext(configuration);
        services.AddUnitOfWorkAndDomainEvents();
        services.AddChatSessionPersistence();
        services.AddMentorPersistence();
        services.AddNotificationPersistence();
        services.AddResourcePersistence();
        services.AddRoadmapPersistence();
        services.AddQuizPersistence();
        services.AddApiKeyPersistence();
        services.AddRefreshTokenPersistence();

        return services;
    }

    public static IServiceCollection AddAppDbContext(this IServiceCollection services, IConfiguration configuration)
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

        return services;
    }

    public static IServiceCollection AddUnitOfWorkAndDomainEvents(this IServiceCollection services)
    {
        services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();
        services.AddScoped<IUnitOfWork, UnitOfWork.UnitOfWork>();

        return services;
    }

    public static IServiceCollection AddChatSessionPersistence(this IServiceCollection services)
    {
        services.AddScoped<IChatSessionRepository, ChatSessionRepository>();
        return services;
    }

    public static IServiceCollection AddMentorPersistence(this IServiceCollection services)
    {
        services.AddScoped<IMentorRepository, MentorRepository>();
        return services;
    }

    public static IServiceCollection AddNotificationPersistence(this IServiceCollection services)
    {
        services.AddScoped<INotificationRepository, NotificationRepository>();
        return services;
    }

    public static IServiceCollection AddResourcePersistence(this IServiceCollection services)
    {
        services.AddScoped<IResourceRepository, ResourceRepository>();
        return services;
    }

    public static IServiceCollection AddRoadmapPersistence(this IServiceCollection services)
    {
        services.AddScoped<IRoadmapRepository, RoadmapRepository>();
        return services;
    }

    public static IServiceCollection AddQuizPersistence(this IServiceCollection services)
    {
        services.AddScoped<IQuizRepository, QuizRepository>();
        return services;
    }

    public static IServiceCollection AddApiKeyPersistence(this IServiceCollection services)
    {
        services.AddScoped<IApiKeyRepository, ApiKeyRepository>();
        return services;
    }

    public static IServiceCollection AddRefreshTokenPersistence(this IServiceCollection services)
    {
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        return services;
    }

    /// <summary>
    /// Registers identity and authentication services.
    /// </summary>
    public static IServiceCollection AddIdentityInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
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
                "Auth:RefreshTokenDays must be greater than 0.");

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

        return services;
    }

    /// <summary>
    /// Registers Azure Blob Storage services.
    /// </summary>
    public static IServiceCollection AddStorageInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<AzureStorageOptions>(configuration.GetSection(AzureStorageOptions.SectionName));

        services.AddSingleton(sp =>
        {
            var options = sp.GetRequiredService<IOptions<AzureStorageOptions>>().Value;
            return new BlobServiceClient(options.ConnectionString);
        });

        services.AddScoped<IFileStorageService, AzureBlobStorageService>();

        return services;
    }

    public static IServiceCollection AddFoundryOptions(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<FoundryOptions>(configuration.GetSection(FoundryOptions.SectionName));
        return services;
    }

    public static IServiceCollection AddEmbeddingInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddFoundryOptions(configuration);
        services.AddSingleton<IEmbeddingService, AzureOpenAIEmbeddingService>();
        return services;
    }

    public static IServiceCollection AddCohereInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddFoundryOptions(configuration);
        services.AddSingleton(sp =>
        {
            var options = sp.GetRequiredService<IOptions<FoundryOptions>>().Value;

            static Uri CreateCohereFoundryBaseUri(string projectEndpoint)
            {
                var endpoint = new Uri(projectEndpoint);

                if (endpoint.AbsolutePath.Contains("/providers/cohere", StringComparison.OrdinalIgnoreCase))
                {
                    return endpoint;
                }

                var root = endpoint.GetLeftPart(UriPartial.Authority);
                return new Uri($"{root}/providers/cohere");
            }

            return new CohereClient(
                httpClient: null,
                baseUri: CreateCohereFoundryBaseUri(options.ProjectEndpoint),
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

        return services;
    }

    public static IServiceCollection AddResourceVectorSearchInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddEmbeddingInfrastructure(configuration);
        services.AddCohereInfrastructure(configuration);
        services.AddScoped<IResourceChunkVectorStore, PgvectorResourceChunkVectorStore>();
        return services;
    }

    public static IServiceCollection AddChunkingInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton<ITokenCountingService, OpenAITokenCountingService>();
        services.AddSingleton<ITextChunkingService, OpenAITextChunkingService>();
        return services;
    }

    public static IServiceCollection AddTavilyInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<TavilyOptions>(configuration.GetSection(TavilyOptions.SectionName));

        services.AddSingleton(sp =>
        {
            var options = sp.GetRequiredService<IOptions<TavilyOptions>>().Value;
            return new TavilyClient(options.ApiKey);
        });

        return services;
    }

    /// <summary>
    /// Registers the AI services needed by resource chat.
    /// </summary>
    public static IServiceCollection AddResourceChatAiInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddResourceVectorSearchInfrastructure(configuration);
        services.AddKeyedScoped<IChatAgent, ResourceChatAgent>("resource-chat");

        return services;
    }

    /// <summary>
    /// Registers AI services: Embeddings, Chunking, Agents, Tavily, Azure OpenAI.
    /// </summary>
    public static IServiceCollection AddFullAiInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddResourceChatAiInfrastructure(configuration);
        services.AddChunkingInfrastructure();
        services.AddTavilyInfrastructure(configuration);

        services.AddKeyedScoped<IChatAgent, GeneralChatAgent>("general-chat");
        services.AddKeyedScoped<IChatAgent, MentorChatAgent>("mentor-chat");
        services.AddScoped<IMentorValidatorAgent, MentorValidatorAgent>();
        services.AddScoped<IAIQuizGenerationWorkflow, QuizAgentWorkflow>();
        services.AddScoped<IAIRoadmapGenerationAgent, AIRoadmapGenerationAgent>();

        return services;
    }

}
