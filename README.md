# Aethria

Aethria is a platform designed for mentorship, learning pathways, and intelligent resource management. It provides tools for team onboarding, knowledge sharing, and interactive learning.

## Key Features

- **Mentorship Management**: Create, assign, and manage mentors to facilitate structured guidance.
- **Learning Roadmaps**: Define and track learning pathways for users to ensure consistent progression.
- **Quizzes**: Evaluate knowledge and progress through integrated quiz functionalities.
- **Intelligent Resource Management**: Upload and process documents (PDF, Text) with AI-powered document chat and extraction capabilities.
- **Team Onboarding**: Manage onboarding processes and view comprehensive data grids and exports.

## Technical Architecture

The project is built on the modern **.NET ecosystem**, leveraging **.NET Aspire** for distributed application orchestration, **Azure** cloud services, and the **Microsoft AI Agent Framework** for intelligent capabilities.

### Core Platform
- **Aethria.Api**: The presentation layer providing RESTful endpoints.
- **Aethria.Application**: Core business logic, CQRS use cases (Commands/Queries), and service abstractions.
- **Aethria.Domain**: Core domain models, entities, and repositories.
- **Aethria.Infrastructure**: Data access, integrations, and external service implementations.
- **Aethria.AppHost & ServiceDefaults**: .NET Aspire configuration for service orchestration and telemetry.
- **Aethria.McpServer**: Model Context Protocol (MCP) server exposing application tools and context for AI agents to consume.

### Client Application
- **aethria.web**: A modern, responsive Single Page Application (SPA) built with **Angular 21**. It connects to the Aethria APIs to deliver a seamless user experience.
