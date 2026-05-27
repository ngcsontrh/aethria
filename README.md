# Aethria

Aethria is an AI-assisted learning platform for managing resources, mentors, quizzes, roadmaps, and chat-based guidance.

## Architecture

The solution follows a Clean Architecture style on .NET 10, with .NET Aspire orchestrating the API, MCP server, and web client.

- `Aethria.Domain`: core entities, value objects, notifications, events, and repository contracts.
- `Aethria.Application`: DispatchR use cases, validation pipelines, storage/vector search abstractions, and API/MCP-specific service registration.
- `Aethria.Infrastructure`: EF Core persistence, Identity, Azure Blob Storage, embeddings, chunking, pgvector search, AI reranking, AI agent integrations, and feature-scoped service registration.
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
│   ├── VectorSearch/            # pgvector resource search and Cohere reranking
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
- Resource upload, parsing, chunking, storage, pgvector search, Cohere reranking, and resource chat.
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

Configuration is supplied through the `appsettings*.json` files for database, Azure Storage, Azure AI Foundry/OpenAI, Cohere reranking, Tavily, and authentication settings.

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
    "AuthGoogleClientId": "<google-oauth-client-id>"
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
  }
}
```

The MCP server uses the same database as the API for API key authentication. Create an API key through `Aethria.Api`, then call the MCP endpoint with the `X-Api-Key` header.

#### Variable Meanings

| Variable | Used by | Meaning |
| --- | --- | --- |
| `DefaultConnection` / `ConnectionStrings:DefaultConnection` | AppHost, API, MCP | PostgreSQL connection string. The database must support the `vector` extension because resource embeddings are stored with pgvector. |
| `FoundryProjectEndpoint` / `Foundry:ProjectEndpoint` | AppHost, API, MCP | Azure AI Foundry project endpoint. The app uses it to build the Cohere provider endpoint for reranking search results. |
| `FoundryAzureOpenAIEndpoint` / `Foundry:AzureOpenAIEndpoint` | AppHost, API, MCP | Azure OpenAI endpoint used for chat, quiz, roadmap, mentor validation, and embedding generation. |
| `FoundryApiKey` / `Foundry:ApiKey` | AppHost, API, MCP | API key for Azure AI Foundry or Azure OpenAI requests. |
| `TavilyApiKey` / `Tavily:ApiKey` | AppHost, API | Tavily API key used by the general chat agent for web search. |
| `AzureStorageConnectionString` / `AzureStorage:ConnectionString` | AppHost, API | Azure Blob Storage connection string used for uploaded learning resources. |
| `AuthIssuer` / `Auth:Issuer` | AppHost, API | JWT issuer value. It must match the issuer used when validating access tokens. |
| `AuthAudience` / `Auth:Audience` | AppHost, API | JWT audience value. For local development, use `Aethria.Api`. |
| `AuthSigningKey` / `Auth:SigningKey` | AppHost, API | Secret key used to sign and validate JWT access tokens. Use a long random value locally. |
| `AuthAccessTokenMinutes` / `Auth:AccessTokenMinutes` | AppHost, API | Access token lifetime in minutes. |
| `AuthRefreshTokenDays` / `Auth:RefreshTokenDays` | AppHost, API | Refresh token lifetime in days. |
| `AuthRefreshTokenCookieName` / `Auth:RefreshTokenCookieName` | AppHost, API | Name of the cookie that stores the refresh token. |
| `AuthGoogleClientId` / `Auth:GoogleClientId` | AppHost, API | Google OAuth client ID used to validate Google sign-in tokens. |
| `Cors` | API | Allowed local web origins. Keep `http://localhost:53174` when using the Aspire-hosted Vite app, or add your own web client origin. |


