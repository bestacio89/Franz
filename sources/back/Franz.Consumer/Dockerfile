# This Dockerfile utilizes multi-stage builds to minimize the final image size
# and to separate the build environment from the runtime environment.
# More info at: https://aka.ms/containerfastmode

# Base image for runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80  # Specify if the application uses HTTP
EXPOSE 443 # Specify if the application uses HTTPS

# Build image for compiling the application
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

# Build the project in Release configuration and output to a folder
RUN dotnet build "Franz.Consumer.csproj" -c Release -o /app/build

# Publish stage to prepare the release artifact
FROM build AS publish
RUN dotnet publish "Franz.Consumer.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final stage/image to be run
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Franz.Consumer.dll"]