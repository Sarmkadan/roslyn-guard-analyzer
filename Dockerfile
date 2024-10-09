# =============================================================================
# Author: Vladyslav Zaiets | https://sarmkadan.com
# CTO & Software Architect
# =============================================================================
# Multi-stage build for Roslyn Guard Analyzer
# Optimized for containerized analysis in CI/CD pipelines
# =============================================================================

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS builder

WORKDIR /build

COPY . .

RUN dotnet restore

RUN dotnet build -c Release --no-restore

RUN dotnet publish -c Release -o /publish --no-build

# Runtime stage
FROM mcr.microsoft.com/dotnet/runtime:10.0

LABEL maintainer="Vladyslav Zaiets <https://sarmkadan.com>"
LABEL description="Roslyn-based code analyzer enforcing architectural rules"
LABEL version="1.2.0"

WORKDIR /app

# Copy published application
COPY --from=builder /publish .

# Create non-root user for security
RUN useradd -m -u 1000 analyzer && \
    chown -R analyzer:analyzer /app

USER analyzer

# Default entrypoint - run analyzer
ENTRYPOINT ["./RoslynGuardAnalyzer"]

# Default command - show help
CMD ["--help"]
