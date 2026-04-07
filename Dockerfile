# =============================================================================
# Author: Vladyslav Zaiets | https://sarmkadan.com
# CTO & Software Architect
# =============================================================================
# Multi-stage build for Roslyn Guard Analyzer v2
# Optimized for containerized analysis with health monitoring
# =============================================================================

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /src

# Copy project files first for layer caching
COPY src/RoslynGuardAnalyzer/RoslynGuardAnalyzer.csproj src/RoslynGuardAnalyzer/
COPY RoslynGuardAnalyzer.sln .

RUN dotnet restore src/RoslynGuardAnalyzer/RoslynGuardAnalyzer.csproj

# Copy everything else
COPY . .

RUN dotnet build src/RoslynGuardAnalyzer/RoslynGuardAnalyzer.csproj -c Release --no-restore

# Publish stage
FROM build AS publish

RUN dotnet publish src/RoslynGuardAnalyzer/RoslynGuardAnalyzer.csproj \
    -c Release \
    -o /app/publish \
    --no-build

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final

LABEL maintainer="Vladyslav Zaiets <https://sarmkadan.com>"
LABEL description="Roslyn-based code analyzer enforcing architectural rules"
LABEL version="2.0.0"

WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080
ENV DOTNET_ENVIRONMENT=Production

EXPOSE 8080

# Copy published application
COPY --from=publish /app/publish .

# Create non-root user for security
RUN useradd -m -u 1000 analyzer && \
    chown -R analyzer:analyzer /app

USER analyzer

HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \
    CMD curl -f http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "RoslynGuardAnalyzer.dll"]
