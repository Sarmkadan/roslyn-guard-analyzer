# Migration Guide - v1.x to v2.0.0

This document covers all breaking changes and required steps to upgrade from Roslyn Guard Analyzer v1.x to v2.0.0.

## Overview

v2.0.0 introduces a containerized HTTP service model alongside the existing CLI. The analyzer now exposes a health endpoint and runs as a long-lived process on port 8080, making it suitable for continuous analysis in CI/CD pipelines and sidecar deployments.

## Breaking Changes

### 1. Docker Image - Runtime Base Changed

**Before (v1.x):**
```dockerfile
FROM mcr.microsoft.com/dotnet/runtime:10.0
```

**After (v2.0):**
```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:10.0
```

The runtime base image switched from `runtime` to `aspnet` to support the new HTTP health endpoint. If you pin base images in your security scans, update your allowlists.

### 2. Docker - Port Exposure

The container now listens on port **8080** by default via `ASPNETCORE_URLS=http://+:8080`.

**Before (v1.x):** No port exposed. CLI-only execution.

**After (v2.0):**
```yaml
ports:
  - "8080:8080"
```

Update firewall rules and reverse proxy configs if deploying behind a gateway.

### 3. Docker Compose - Version Field Removed

The `version: '3.8'` field has been removed from `docker-compose.yml` per Docker Compose v2 spec (the field is deprecated and ignored since Compose v2.0).

If you have tooling that parses the version field, update it.

### 4. Docker Compose - Service Health Dependencies

The `results-viewer` service now depends on `analyzer` with `condition: service_healthy` instead of a simple `depends_on`. This means the viewer will not start until the analyzer passes its health check.

### 5. HEALTHCHECK Added

The Dockerfile now includes a `HEALTHCHECK` instruction:

```dockerfile
HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \
    CMD curl -f http://localhost:8080/health || exit 1
```

Orchestrators (Docker Swarm, Kubernetes via `livenessProbe` mapping) will now monitor container health automatically.

### 6. Entrypoint Changed

**Before (v1.x):**
```dockerfile
ENTRYPOINT ["./RoslynGuardAnalyzer"]
CMD ["--help"]
```

**After (v2.0):**
```dockerfile
ENTRYPOINT ["dotnet", "RoslynGuardAnalyzer.dll"]
```

If you override the entrypoint in your compose or Kubernetes manifests, update accordingly.

### 7. Docker Build - Layer Caching Optimization

The Dockerfile now copies `.csproj` files and runs `dotnet restore` before copying the full source. This means rebuilds that only change source code will not re-download NuGet packages.

No action required unless your CI caches Docker layers - you may see improved build times.

## Non-Breaking Changes

- Image tag updated from `latest` to `2.0.0` in docker-compose
- Results viewer port changed from `8080` to `9090` to avoid conflict with analyzer
- Container restart policy set to `unless-stopped`
- `user: analyzer` removed from compose (handled inside Dockerfile via `USER` instruction)

## Step-by-Step Migration

1. **Update your local copy:**
   ```bash
   git pull origin main
   ```

2. **Rebuild the Docker image:**
   ```bash
   docker compose build --no-cache
   ```

3. **Update any port mappings** in your deployment configs from no port to `8080`.

4. **Update health check endpoints** in your orchestrator to point to `http://localhost:8080/health`.

5. **Verify the upgrade:**
   ```bash
   docker compose up -d
   docker compose ps   # Should show "healthy" status
   curl http://localhost:8080/health
   ```

## Rollback

If you need to roll back to v1.x:

```bash
git checkout v1.0.0
docker compose build
docker compose up -d
```

## Questions

Open an issue at [GitHub Issues](https://github.com/sarmkadan/roslyn-guard-analyzer/issues) if you encounter problems during migration.
