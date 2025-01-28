# Base image for running the application (for .NET 9, update when available)
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base   # Update for .NET 9 (when released)
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Build image (for .NET 9, update when available)
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build   # Update for .NET 9 (when released)
WORKDIR /src

# Copy the project file and restore dependencies
COPY ["FranzMVC.ASPNET/FranzMVC.ASPNET.csproj", "FranzMVC.ASPNET/"]
RUN dotnet restore "FranzMVC.ASPNET/FranzMVC.ASPNET.csproj"

# Copy the rest of the application and build it
COPY . . 
WORKDIR "/src/FranzMVC.ASPNET"
RUN dotnet build "FranzMVC.ASPNET.csproj" -c Release -o /app/build

# Publish the app (for .NET 9, update when available)
FROM build AS publish
RUN dotnet publish "FranzMVC.ASPNET.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final image for running the app (for .NET 9, update when available)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish . 
ENTRYPOINT ["dotnet", "FranzMVC.ASPNET.dll"]
