# Base stage for runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80 
EXPOSE 443 

# Add a health check to ensure the container is responsive
HEALTHCHECK --interval=30s --timeout=30s --start-period=5s --retries=3 \
  CMD curl --silent --fail http://localhost:80/health || exit 1

# Build stage for compiling the application
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy NuGet configuration
COPY ["nuget.config", "."]

# Copy project files separately to leverage Docker cache
COPY ["sources/back/Franz.Consumer/Franz.Consumer.csproj", "sources/back/Franz.Consumer/"]
COPY ["sources/back/Franz.Persistence/Franz.Persistence.csproj", "sources/back/Franz.Persistence/"]
COPY ["sources/back/Franz.Domain/Franz.Domain.csproj", "sources/back/Franz.Domain/"]
COPY ["sources/back/Franz.Application/Franz.Application.csproj", "sources/back/Franz.Application/"]
COPY ["sources/back/Franz.Contracts/Franz.Contracts.csproj", "sources/back/Franz.Contracts/"]

# Restore dependencies
RUN dotnet restore "sources/back/Franz.Consumer/Franz.Consumer.csproj"

# Copy the rest of the solution's files
COPY . .

# Set the working directory to the Consumer project's directory
WORKDIR "/src/sources/back/Franz.Consumer"
RUN dotnet build "Franz.Consumer.csproj" -c Release -o /app/build

# Publish stage to prepare the release artifact
FROM build AS publish
RUN dotnet publish "Franz.Consumer.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final stage/image to be run
FROM base AS final
WORKDIR /app

# Copy the published output from the publish stage
COPY --from=publish /app/publish .

# Create a non-root user to improve security
RUN useradd -m myuser
USER myuser

# Define the entry point to start the application
ENTRYPOINT ["dotnet", "Franz.Consumer.dll"]
