# Aethria

Aethria is an AI-assisted learning platform for managing resources, mentors, quizzes, roadmaps, and chat-based guidance.

## Architecture

The solution follows a Clean Architecture style on .NET 10, with .NET Aspire orchestrating the API, MCP server, and web client.

- `Aethria.Domain`: core entities, value objects, events, and repository contracts.
- `Aethria.Application`: use cases, validation, abstractions, and CQRS-style commands/queries.
- `Aethria.Infrastructure`: EF Core persistence, Identity, Azure Blob Storage, embeddings, chunking, and AI agent integrations.
- `Aethria.Api`: REST endpoints, SignalR hubs, JWT authentication, OpenAPI, and Scalar UI.
- `Aethria.McpServer`: protected Model Context Protocol server for exposing Aethria tools to AI agents.
- `Aethria.AppHost`: .NET Aspire host for local distributed orchestration.
- `aethria.web`: React 19 + TypeScript + Vite SPA using Mantine, TanStack Router, React Query, SignalR, and i18next.

## Main Capabilities

- Email and Google authentication with refresh tokens.
- Resource upload, parsing, chunking, storage, search, and resource chat.
- Mentor creation, validation, and mentor chat.
- AI-generated learning roadmaps with streaming updates.
- Quiz creation, AI quiz generation, attempts, submission history, and review.
- API key management for MCP access.

## Run Locally

Prerequisites: .NET 10 SDK and Node.js.

```bash
dotnet restore
dotnet run --project Aethria.AppHost
```

Configuration is supplied through Aspire parameters and the `appsettings*.json` files for database, Azure Storage, Azure AI Foundry/OpenAI, Tavily, and authentication settings.
