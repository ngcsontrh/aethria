FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
RUN apt-get update \
    && apt-get install -y --no-install-recommends libgssapi-krb5-2 \
    && rm -rf /var/lib/apt/lists/*
ENV ASPNETCORE_HTTP_PORTS=8080
USER $APP_UID
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /build
COPY ["Directory.Packages.props", "."]
COPY ["Aethria.McpServer/Aethria.McpServer.csproj", "Aethria.McpServer/"]
COPY ["Aethria.Application/Aethria.Application.csproj", "Aethria.Application/"]
COPY ["Aethria.Domain/Aethria.Domain.csproj", "Aethria.Domain/"]
COPY ["Aethria.Infrastructure/Aethria.Infrastructure.csproj", "Aethria.Infrastructure/"]
COPY ["Aethria.ServiceDefaults/Aethria.ServiceDefaults.csproj", "Aethria.ServiceDefaults/"]
RUN dotnet restore "./Aethria.McpServer/Aethria.McpServer.csproj"
COPY . .
WORKDIR "/build/Aethria.McpServer"
RUN dotnet build "./Aethria.McpServer.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Aethria.McpServer.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Aethria.McpServer.dll"]
