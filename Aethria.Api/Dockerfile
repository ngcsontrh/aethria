FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
ENV ASPNETCORE_HTTP_PORTS=8080
USER $APP_UID
WORKDIR /app
EXPOSE 8080


FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /build
COPY ["Directory.Packages.props", "."]
COPY ["Aethria.Api/Aethria.Api.csproj", "Aethria.Api/"]
COPY ["Aethria.ServiceDefaults/Aethria.ServiceDefaults.csproj", "Aethria.ServiceDefaults/"]
COPY ["Aethria.Infrastructure/Aethria.Infrastructure.csproj", "Aethria.Infrastructure/"]
COPY ["Aethria.Application/Aethria.Application.csproj", "Aethria.Application/"]
COPY ["Aethria.Domain/Aethria.Domain.csproj", "Aethria.Domain/"]
RUN dotnet restore "./Aethria.Api/Aethria.Api.csproj"
COPY . .
WORKDIR "/build/Aethria.Api"
RUN dotnet build "./Aethria.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Aethria.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Aethria.Api.dll"]
