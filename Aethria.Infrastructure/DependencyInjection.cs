using Aethria.Application.Abstractions.Chunking;
using Aethria.Application.Abstractions.Embedding;
using Aethria.Application.Abstractions.Identity;
using Aethria.Application.Abstractions.Persistence;
using Aethria.Application.Abstractions.Storage;
using Aethria.Application.UseCases.Chat.Contracts;
using Aethria.Application.UseCases.Mentors;
using Aethria.Application.UseCases.Quizzes.CreateAIQuizStream;
using Aethria.Application.UseCases.Roadmaps.GenerateAIRoadmapStream;
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
using Azure.Storage.Blobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Tavily;

namespace Aethria.Infrastructure;

public static class DependencyInjection
{
    /// <summary>
    /// Registers all infrastructure services. Use this for projects that need the full stack.
    /// </summary>
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddInfrastructurePersistence(configuration);
        services.AddInfrastructureIdentity(configuration);
        services.AddInfrastructureStorage(configuration);
        services.AddInfrastructureAI(configuration);

        return services;
    }

    /// <summary>
    /// Registers persistence services: DbContext, Repositories, UnitOfWork, Domain events.
    /// Sufficient for worker services that only need database access.
    /// </summary>
    public static IServiceCollection AddInfrastructurePersistence(this IServiceCollection services, IConfiguration configuration)
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

        services.AddScoped<IChatSessionRepository, ChatSessionRepository>();
        services.AddScoped<IMentorRepository, MentorRepository>();
        services.AddScoped<INotificationRepository, NotificationRepository>();
        services.AddScoped<IResourceRepository, ResourceRepository>();
        services.AddScoped<IResourceChunkRepository, ResourceChunkRepository>();
        services.AddScoped<IRoadmapRepository, RoadmapRepository>();
        services.AddScoped<IQuizRepository, QuizRepository>();
        services.AddScoped<IApiKeyRepository, ApiKeyRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();

        services.AddScoped<IUnitOfWork, UnitOfWork.UnitOfWork>();

        return services;
    }

    /// <summary>
    /// Registers identity and authentication services.
    /// </summary>
    public static IServiceCollection AddInfrastructureIdentity(this IServiceCollection services, IConfiguration configuration)
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
                "Auth:RefreshTokenDays must be greater than 0.")
            .ValidateOnStart();

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
    public static IServiceCollection AddInfrastructureStorage(this IServiceCollection services, IConfiguration configuration)
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

    /// <summary>
    /// Registers AI services: Embeddings, Chunking, Agents, Tavily, Azure OpenAI.
    /// </summary>
    public static IServiceCollection AddInfrastructureAI(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<FoundryOptions>(configuration.GetSection(FoundryOptions.SectionName));
        services.Configure<TavilyOptions>(configuration.GetSection(TavilyOptions.SectionName));

        services.AddSingleton<ITokenCountingService, OpenAITokenCountingService>();
        services.AddSingleton<ITextChunkingService, OpenAITextChunkingService>();

        services.AddSingleton<IEmbeddingService, AzureOpenAIEmbeddingService>();

        services.AddSingleton(sp =>
        {
            var options = sp.GetRequiredService<IOptions<TavilyOptions>>().Value;
            return new TavilyClient(options.ApiKey);
        });

        services.AddKeyedScoped<IChatAgent, GeneralChatAgent>("general-chat");
        services.AddKeyedScoped<IChatAgent, MentorChatAgent>("mentor-chat");
        services.AddKeyedScoped<IChatAgent, ResourceChatAgent>("resource-chat");
        services.AddScoped<IMentorValidatorAgent, MentorValidatorAgent>();
        services.AddScoped<IAIQuizGenerationWorkflow, QuizAgentWorkflow>();
        services.AddScoped<IAIRoadmapGenerationAgent, AIRoadmapGenerationAgent>();

        return services;
    }
}
