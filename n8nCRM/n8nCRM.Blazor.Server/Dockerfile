# =============================================================================
# Build stage
# =============================================================================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

ARG DEVEXPRESS_NUGET_URL

# Add the DevExpress NuGet feed
RUN dotnet nuget add source "$DEVEXPRESS_NUGET_URL" -n DevExpress

WORKDIR /src

# Copy central package management and project files first (layer caching)
COPY n8nCRM/Directory.packages.props n8nCRM/Directory.packages.props
COPY n8nCRM/n8nCRM.Module/n8nCRM.Module.csproj n8nCRM/n8nCRM.Module/
COPY n8nCRM/n8nCRM.Blazor.Server/n8nCRM.Blazor.Server.csproj n8nCRM/n8nCRM.Blazor.Server/

# Restore dependencies
RUN dotnet restore n8nCRM/n8nCRM.Blazor.Server/n8nCRM.Blazor.Server.csproj

# Copy everything else
COPY . .

# Publish the Blazor Server app
RUN dotnet publish n8nCRM/n8nCRM.Blazor.Server/n8nCRM.Blazor.Server.csproj -c Release -o /app

# =============================================================================
# Runtime stage
# =============================================================================
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime

# Install libicu for globalization support
RUN apt-get update && apt-get install -y libicu-dev && rm -rf /var/lib/apt/lists/*

WORKDIR /app

# Copy published output from the build stage
COPY --from=build /app .

# Environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Development
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

EXPOSE 8080

ENTRYPOINT ["dotnet", "n8nCRM.Blazor.Server.dll"]
