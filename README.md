# Aethria

Aethria is an AI-assisted learning platform for managing resources, mentors, quizzes, roadmaps, and chat-based guidance.

## Architecture

The solution follows a Clean Architecture style on .NET 10, with .NET Aspire orchestrating the API, MCP server, and web client.

- `Aethria.Domain`: core entities, value objects, notifications, events, and repository contracts.
- `Aethria.Application`: DispatchR use cases, validation pipelines, storage/vector search abstractions, and API/MCP-specific service registration.
- `Aethria.Infrastructure`: EF Core persistence, Identity, Azure Blob Storage, embeddings, chunking, Qdrant vector search, AI reranking, AI agent integrations, and feature-scoped service registration.
- `Aethria.Api`: REST endpoints for app features, notifications, SignalR hubs, JWT authentication, OpenAPI, Scalar UI, and full API service composition.
- `Aethria.McpServer`: protected Model Context Protocol server with minimal resource-chat service composition for AI agents.
- `Aethria.AppHost`: .NET Aspire host for local distributed orchestration.
- `Aethria.ServiceDefaults`: shared configurations for telemetry, resilience, and service discovery.
- `aethria.web`: React 19 + TypeScript + Vite SPA using Mantine, TanStack Router, React Query, SignalR, and i18next.

## Project Structure

```text
aethria/
├── Aethria.AppHost/             # .NET Aspire host for local distributed orchestration
│   ├── AppHost.cs               # Service orchestration setup
│   └── appsettings.json         # Local configuration / resources
├── Aethria.Api/                 # REST endpoints, SignalR hubs, JWT authentication, and Scalar UI
│   ├── Authentication/          # User authentication endpoints & configurations
│   ├── Endpoints/               # API endpoints grouped by domain feature
│   ├── Hubs/                    # SignalR hub for real-time notifications
│   └── Program.cs               # API entry point using AddApi* service registrations
├── Aethria.McpServer/           # Protected Model Context Protocol (MCP) server for AI integrations
│   ├── Authentication/          # API key validation for AI clients
│   ├── Tools/                   # Tools exposed to Model Context Protocol clients
│   └── Program.cs               # MCP entry point using AddMcp* minimal registrations
├── Aethria.Domain/              # Core domain layers (Entities, Events, Repositories contracts)
│   ├── Common/                  # Shared domain base classes
│   ├── Entities/                # Enterprise/domain logic models (User, Resource, Mentor, Quiz, Roadmap, Notification)
│   ├── Events/                  # Domain events (e.g. ResourceUploadedEvent)
│   ├── Repositories/            # Interfaces defining persistence operations
│   └── ValueObjects/            # Domain value objects and typed constants
├── Aethria.Application/         # DispatchR handlers, validation, use cases, and interfaces
│   ├── Abstractions/            # Interfaces for external dependencies (Storage, Embeddings, Vector Search)
│   ├── Behaviors/               # DispatchR validation pipeline behaviors
│   ├── Utils/                   # Shared parsing and text sanitization helpers
│   ├── DependencyInjection.cs   # AddApiApplicationServices and AddMcpApplicationServices
│   └── UseCases/                # Commands/queries, stream requests, notifications, and handlers
├── Aethria.Infrastructure/      # Database persistence, Blob Storage, AI agent integrations
│   ├── AgentFramework/          # Orchestrators and tools for LLM interactions
│   ├── Chunking/                # PDF/TXT document parsing and chunking policies
│   ├── Configuration/           # Options bound from host configuration
│   ├── DomainEvents/            # DispatchR domain event dispatcher
│   ├── Embedding/               # Semantic vectors generation implementations
│   ├── Identity/                # ASP.NET Identity and token services
│   ├── Repositories/            # EF Core repository implementations
│   ├── Persistence/             # Entity Framework Core DbContext configurations
│   ├── Storage/                 # File uploads to Azure Blob Storage
│   ├── UnitOfWork/              # Unit of work and transaction coordination
│   ├── VectorSearch/            # Qdrant resource search and Cohere reranking
│   └── DependencyInjection.cs   # AddApiInfrastructureServices and AddMcpInfrastructureServices
├── Aethria.ServiceDefaults/     # Shared resiliency, service discovery, telemetry, and health check config
│   └── Extensions.cs            # Custom IHostApplicationBuilder setups
└── aethria.web/                 # Front-end SPA: React 19 + TypeScript + Vite + Mantine
    ├── public/                  # Static assets and translations
    └── src/
        ├── components/          # Reusable UI components
        ├── i18n/                # Localization / Internationalization (i18next)
        ├── pages/               # Routing pages & views
        ├── services/            # API clients, React Query queries, and SignalR hooks
        ├── main.tsx             # React mount entrypoint
        └── router.tsx           # TanStack Router configuration
```

## Main Capabilities

- Email and Google authentication with refresh tokens.
- Resource upload, parsing, chunking, storage, Qdrant vector search, Cohere reranking, and resource chat.
- Mentor creation, validation, and mentor chat.
- AI-generated learning roadmaps with streaming updates.
- Quiz creation, AI quiz generation, attempts, submission history, and review.
- Localized notification center for generated quizzes, roadmaps, and uploaded resources.
- API key management for MCP access.

## Run Locally

Prerequisites: .NET 10 SDK and Node.js.

```bash
dotnet restore
dotnet run --project Aethria.AppHost
```

Configuration is supplied through the `appsettings*.json` files for database, Qdrant, Azure Storage, Azure AI Foundry/OpenAI, Cohere reranking, Tavily, and authentication settings.

## Run Published Container Images

The API and MCP server are published as public images on GitHub Container Registry (GHCR). No registry login is required.

| Service | Image |
| --- | --- |
| API | `ghcr.io/ngcsontrh/aethria/aethria-api:latest` |
| MCP server | `ghcr.io/ngcsontrh/aethria/aethria-mcpserver:latest` |

Versioned images are also published from release tags. For example, pushing `api-v1.0.0` publishes `ghcr.io/ngcsontrh/aethria/aethria-api:v1.0.0`, and pushing `mcpserver-v1.0.0` publishes `ghcr.io/ngcsontrh/aethria/aethria-mcpserver:v1.0.0`.

Pull the images:

```bash
docker pull ghcr.io/ngcsontrh/aethria/aethria-api:latest
docker pull ghcr.io/ngcsontrh/aethria/aethria-mcpserver:latest
```

To run with Docker Compose, create a `docker-compose.yml` that references these images, fill the environment variables listed in the configuration tables below, then start the services:

```bash
docker compose pull aethria-api aethria-mcpserver
docker compose up -d aethria-api aethria-mcpserver
```

To run without Compose, place the required API settings in `api.env` and the MCP server settings in `mcpserver.env`, then run:

```bash
docker run -d \
  --name aethria-api \
  --restart unless-stopped \
  -p 5250:8080 \
  --env-file api.env \
  ghcr.io/ngcsontrh/aethria/aethria-api:latest

docker run -d \
  --name aethria-mcpserver \
  --restart unless-stopped \
  -p 5261:8080 \
  --env-file mcpserver.env \
  ghcr.io/ngcsontrh/aethria/aethria-mcpserver:latest
```

Use the environment variable names from the `Aethria.Api` and `Aethria.McpServer` tables below. At minimum, set the database, Foundry/OpenAI, Qdrant, authentication, storage, and CORS values required by the service you are starting.

### Local Configuration

For local development, fill the variables in each project's `appsettings.Development.json`.

#### Aethria.AppHost

`Aethria.AppHost` is the recommended local entry point. Fill `Aethria.AppHost/appsettings.Development.json` under `Parameters`. AppHost reads these Aspire parameters and passes them to the services it starts.

```json
{
  "Parameters": {
    "DefaultConnection": "<postgres-connection-string>",
    "FoundryProjectEndpoint": "<azure-ai-foundry-project-endpoint>",
    "FoundryAzureOpenAIEndpoint": "<azure-openai-endpoint>",
    "FoundryApiKey": "<azure-ai-foundry-or-openai-api-key>",
    "TavilyApiKey": "<tavily-api-key>",
    "AzureStorageConnectionString": "<azure-storage-connection-string>",
    "AuthIssuer": "Aethria",
    "AuthAudience": "Aethria.Api",
    "AuthSigningKey": "<long-local-signing-key>",
    "AuthAccessTokenMinutes": "15",
    "AuthRefreshTokenDays": "7",
    "AuthRefreshTokenCookieName": "aethria.refresh_token",
    "AuthGoogleClientId": "<google-oauth-client-id>",
    "QdrantEndpoint": "<qdrant-endpoint>",
    "QdrantApiKey": "<qdrant-api-key>"
  }
}
```

#### Aethria.Api

When running the API directly with `dotnet run --project Aethria.Api`, fill `Aethria.Api/appsettings.Development.json`.

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "<postgres-connection-string>"
  },
  "Foundry": {
    "ProjectEndpoint": "<azure-ai-foundry-project-endpoint>",
    "AzureOpenAIEndpoint": "<azure-openai-endpoint>",
    "ApiKey": "<azure-ai-foundry-or-openai-api-key>"
  },
  "Tavily": {
    "ApiKey": "<tavily-api-key>"
  },
  "AzureStorage": {
    "ConnectionString": "<azure-storage-connection-string>"
  },
  "Qdrant": {
    "Endpoint": "<qdrant-endpoint>",
    "ApiKey": "<qdrant-api-key>"
  },
  "Auth": {
    "Issuer": "Aethria",
    "Audience": "Aethria.Api",
    "SigningKey": "<long-local-signing-key>",
    "AccessTokenMinutes": 15,
    "RefreshTokenDays": 7,
    "RefreshTokenCookieName": "aethria.refresh_token",
    "GoogleClientId": "<google-oauth-client-id>"
  }
}
```

Equivalent environment variables for `Aethria.Api`:

| Environment variable | Required | Value |
| --- | --- | --- |
| `ConnectionStrings__DefaultConnection` | Yes | PostgreSQL connection string. |
| `Foundry__ProjectEndpoint` | Yes | Azure AI Foundry project endpoint for Foundry-hosted model/provider APIs. |
| `Foundry__AzureOpenAIEndpoint` | Yes | Azure OpenAI endpoint for deployed OpenAI-compatible models. |
| `Foundry__ApiKey` | Yes | Azure AI Foundry / Azure OpenAI API key. |
| `Tavily__ApiKey` | Yes | Tavily API key used by chat agents that can search the web. |
| `AzureStorage__ConnectionString` | Yes | Azure Blob Storage connection string for uploaded resources. |
| `Qdrant__Endpoint` | Yes | Qdrant endpoint for resource chunk vectors. |
| `Qdrant__ApiKey` | Yes | Qdrant API key. |
| `Auth__Issuer` | Yes | JWT issuer. |
| `Auth__Audience` | Yes | JWT audience, normally `Aethria.Api`. |
| `Auth__SigningKey` | Yes | Long secret used to sign JWT access tokens. |
| `Auth__AccessTokenMinutes` | Yes | Access token lifetime in minutes. |
| `Auth__RefreshTokenDays` | Yes | Refresh token lifetime in days. |
| `Auth__RefreshTokenCookieName` | Yes | Cookie name used for refresh tokens. |
| `Auth__GoogleClientId` | Yes | Google OAuth client ID used to validate Google ID tokens. |
| `Cors__0__Origins__0` | Optional | First allowed web origin, for example `http://localhost:53174`. Add more origins as `Cors__0__Origins__1`, etc. |
| `Cors__0__AllowedMethods__0` | Optional | Use `*` for all methods in local development. |
| `Cors__0__AllowedHeaders__0` | Optional | Use `*` for all headers in local development. |
| `Cors__0__AllowCredentials` | Optional | Set to `true` when the browser must send refresh-token cookies. |
| `ASPNETCORE_ENVIRONMENT` | Optional | Set to `Development` for local development. |

If the web client runs on a different local origin, add it to `Cors` in `Aethria.Api/appsettings.Development.json`.

#### Aethria.McpServer

When running the MCP server directly with `dotnet run --project Aethria.McpServer`, fill `Aethria.McpServer/appsettings.Development.json`.

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "<postgres-connection-string>"
  },
  "Foundry": {
    "ProjectEndpoint": "<azure-ai-foundry-project-endpoint>",
    "AzureOpenAIEndpoint": "<azure-openai-endpoint>",
    "ApiKey": "<azure-ai-foundry-or-openai-api-key>"
  },
  "Qdrant": {
    "Endpoint": "<qdrant-endpoint>",
    "ApiKey": "<qdrant-api-key>"
  }
}
```

Equivalent environment variables for `Aethria.McpServer`:

| Environment variable | Required | Value |
| --- | --- | --- |
| `ConnectionStrings__DefaultConnection` | Yes | PostgreSQL connection string. Use the same database as `Aethria.Api` so API keys can be validated. |
| `Foundry__ProjectEndpoint` | Yes | Azure AI Foundry project endpoint for Foundry-hosted model/provider APIs. |
| `Foundry__AzureOpenAIEndpoint` | Yes | Azure OpenAI endpoint for deployed OpenAI-compatible models. |
| `Foundry__ApiKey` | Yes | Azure AI Foundry / Azure OpenAI API key. |
| `Qdrant__Endpoint` | Yes | Qdrant endpoint for resource chunk vectors. |
| `Qdrant__ApiKey` | Yes | Qdrant API key. |
| `ASPNETCORE_ENVIRONMENT` | Optional | Set to `Development` for local development. |

The MCP server uses the same database as the API for API key authentication. Create an API key through `Aethria.Api`, then call the MCP endpoint with the `X-Api-Key` header.

#### Variable Meanings

| AppHost parameter / appsettings key / environment variable | Used by | Meaning |
| --- | --- | --- |
| `DefaultConnection` / `ConnectionStrings:DefaultConnection` / `ConnectionStrings__DefaultConnection` | AppHost, API, MCP | PostgreSQL connection string used for application data, identity, API keys, resources, quizzes, roadmaps, and notifications. |
| `QdrantEndpoint` / `Qdrant:Endpoint` / `Qdrant__Endpoint` | AppHost, API, MCP | Qdrant endpoint used to store and search resource chunk embeddings. |
| `QdrantApiKey` / `Qdrant:ApiKey` / `Qdrant__ApiKey` | AppHost, API, MCP | API key used to authenticate Qdrant requests. |
| `FoundryProjectEndpoint` / `Foundry:ProjectEndpoint` / `Foundry__ProjectEndpoint` | AppHost, API, MCP | Azure AI Foundry project endpoint for Foundry-hosted model/provider APIs. |
| `FoundryAzureOpenAIEndpoint` / `Foundry:AzureOpenAIEndpoint` / `Foundry__AzureOpenAIEndpoint` | AppHost, API, MCP | Azure OpenAI endpoint for deployed OpenAI-compatible models. |
| `FoundryApiKey` / `Foundry:ApiKey` / `Foundry__ApiKey` | AppHost, API, MCP | API key for Azure AI Foundry or Azure OpenAI requests. |
| `TavilyApiKey` / `Tavily:ApiKey` / `Tavily__ApiKey` | AppHost, API | Tavily API key used by the general chat agent for web search. |
| `AzureStorageConnectionString` / `AzureStorage:ConnectionString` / `AzureStorage__ConnectionString` | AppHost, API | Azure Blob Storage connection string used for uploaded learning resources. |
| `AuthIssuer` / `Auth:Issuer` / `Auth__Issuer` | AppHost, API | JWT issuer value. It must match the issuer used when validating access tokens. |
| `AuthAudience` / `Auth:Audience` / `Auth__Audience` | AppHost, API | JWT audience value. For local development, use `Aethria.Api`. |
| `AuthSigningKey` / `Auth:SigningKey` / `Auth__SigningKey` | AppHost, API | Secret key used to sign and validate JWT access tokens. Use a long random value locally. |
| `AuthAccessTokenMinutes` / `Auth:AccessTokenMinutes` / `Auth__AccessTokenMinutes` | AppHost, API | Access token lifetime in minutes. |
| `AuthRefreshTokenDays` / `Auth:RefreshTokenDays` / `Auth__RefreshTokenDays` | AppHost, API | Refresh token lifetime in days. |
| `AuthRefreshTokenCookieName` / `Auth:RefreshTokenCookieName` / `Auth__RefreshTokenCookieName` | AppHost, API | Name of the cookie that stores the refresh token. |
| `AuthGoogleClientId` / `Auth:GoogleClientId` / `Auth__GoogleClientId` | AppHost, API | Google OAuth client ID used to validate Google sign-in tokens. |
| `Cors` | API | Allowed local web origins. Keep `http://localhost:53174` when using the Aspire-hosted Vite app, or add your own web client origin. |


