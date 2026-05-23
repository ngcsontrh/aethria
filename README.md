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

Configuration is supplied through Aspire parameters and the `appsettings*.json` files for database, Azure Storage, Azure AI Foundry/OpenAI, Cohere reranking, Tavily, and authentication settings.

