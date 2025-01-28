# Base stage for runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0-windowsservercore-ltsc2019 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Set the HEALTHCHECK to ensure container health
HEALTHCHECK --interval=30s --timeout=30s --start-period=5s --retries=3 \
  CMD curl --silent --fail http://localhost:80/health || exit 1

# SDK stage for building the application
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy nuget config and necessary csproj files
COPY ["nuget.config", "."]
COPY ["sources/back/Franz.API/Franz.API.csproj", "sources/back/Franz.API/"]
COPY ["sources/back/Franz.Persistence/Franz.Persistence.csproj", "sources/back/Franz.Persistence/"]
COPY ["sources/back/Franz.Domain/Franz.Domain.csproj", "sources/back/Franz.Domain/"]
COPY ["sources/back/Franz.Application/Franz.Application.csproj", "sources/back/Franz.Application/"]
COPY ["sources/back/Franz.Contracts/Franz.Contracts.csproj", "sources/back/Franz.Contracts/"]

# Restore dependencies
RUN dotnet restore "sources/back/Franz.API/Franz.API.csproj"

# Copy the rest of the source code
COPY . .

# Build the application
WORKDIR "/src/sources/back/Franz.API"
RUN dotnet build "Franz.API.csproj" -c Release -o /app/build

# Publish stage for preparing the release artifact
FROM build AS publish
RUN dotnet publish "Franz.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final stage for creating the runtime image
FROM base AS final
WORKDIR /app

# Copy the build artifact from the publish stage
COPY --from=publish /app/publish .

# Create a non-root user to enhance security
RUN useradd -m myuser
USER myuser

# Set the entrypoint for the application
ENTRYPOINT ["dotnet", "Franz.API.dll"]
