var builder = DistributedApplication.CreateBuilder(args);

var defaultConnection = builder.AddParameter("DefaultConnection", secret: true);
var foundryProjectEndpoint = builder.AddParameter("FoundryProjectEndpoint", secret: true);
var foundryAzureOpenAIEndpoint = builder.AddParameter("FoundryAzureOpenAIEndpoint", secret: true);
var foundryApiKey = builder.AddParameter("FoundryApiKey", secret: true);
var tavilyApiKey = builder.AddParameter("TavilyApiKey", secret: true);
var azureStorageConnectionString = builder.AddParameter("AzureStorageConnectionString", secret: true);
var authIssuer = builder.AddParameter("AuthIssuer", secret: true);
var authAudience = builder.AddParameter("AuthAudience", secret: true);
var authSigningKey = builder.AddParameter("AuthSigningKey", secret: true);
var authAccessTokenMinutes = builder.AddParameter("AuthAccessTokenMinutes", secret: true);
var authRefreshTokenDays = builder.AddParameter("AuthRefreshTokenDays", secret: true);
var authRefreshTokenCookieName = builder.AddParameter("AuthRefreshTokenCookieName", secret: true);
var authGoogleClientId = builder.AddParameter("AuthGoogleClientId", secret: true);
var qdrantEndpoint = builder.AddParameter("QdrantEndpoint", secret: true);
var qdrantApiKey = builder.AddParameter("QdrantApiKey", secret: true);

builder.AddProject<Projects.Aethria_Api>("aethria-api")
       .WithEnvironment("ConnectionStrings__DefaultConnection", defaultConnection)
       .WithEnvironment("Foundry__ProjectEndpoint", foundryProjectEndpoint)
       .WithEnvironment("Foundry__AzureOpenAIEndpoint", foundryAzureOpenAIEndpoint)
       .WithEnvironment("Foundry__ApiKey", foundryApiKey)
       .WithEnvironment("Tavily__ApiKey", tavilyApiKey)
       .WithEnvironment("AzureStorage__ConnectionString", azureStorageConnectionString)
       .WithEnvironment("Auth__Issuer", authIssuer)
       .WithEnvironment("Auth__Audience", authAudience)
       .WithEnvironment("Auth__SigningKey", authSigningKey)
       .WithEnvironment("Auth__AccessTokenMinutes", authAccessTokenMinutes)
       .WithEnvironment("Auth__RefreshTokenDays", authRefreshTokenDays)
       .WithEnvironment("Auth__RefreshTokenCookieName", authRefreshTokenCookieName)
       .WithEnvironment("Auth__GoogleClientId", authGoogleClientId)
       .WithEnvironment("Qdrant__Endpoint", qdrantEndpoint)
       .WithEnvironment("Qdrant__ApiKey", qdrantApiKey);

builder.AddProject<Projects.Aethria_McpServer>("aethria-mcpserver")
       .WithEnvironment("ConnectionStrings__DefaultConnection", defaultConnection)
       .WithEnvironment("Foundry__AzureOpenAIEndpoint", foundryAzureOpenAIEndpoint)
       .WithEnvironment("Foundry__ApiKey", foundryApiKey)
       .WithEnvironment("Qdrant__Endpoint", qdrantEndpoint)
       .WithEnvironment("Qdrant__ApiKey", qdrantApiKey);

builder.AddViteApp("aethria-web", "../aethria.web")
       .WithHttpEndpoint(port: 53174)
       .WithEnvironment("VITE_GOOGLE_CLIENT_ID", authGoogleClientId)
       .WithExternalHttpEndpoints();

builder.Build().Run();
