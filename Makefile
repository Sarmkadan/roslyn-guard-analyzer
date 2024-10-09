.PHONY: help build rebuild release test clean analyze publish install docs format watch

# Colors for terminal output
GREEN := \033[0;32m
YELLOW := \033[0;33m
BLUE := \033[0;34m
NC := \033[0m # No Color

# Defaults
CONFIGURATION ?= Debug
PROJECT_DIR := src/RoslynGuardAnalyzer
SOLUTION_FILE := RoslynGuardAnalyzer.sln

help: ## Display this help message
	@echo "$(BLUE)Roslyn Guard Analyzer - Build Commands$(NC)"
	@echo ""
	@grep -E '^[a-zA-Z_-]+:.*?## .*$$' $(MAKEFILE_LIST) | sort | awk 'BEGIN {FS = ":.*?## "}; {printf "$(GREEN)%-20s$(NC) %s\n", $$1, $$2}'
	@echo ""
	@echo "$(YELLOW)Usage:$(NC) make [target] [CONFIGURATION=Release]"
	@echo ""

## Build targets
build: ## Build project in Debug configuration
	@echo "$(YELLOW)Building $(CONFIGURATION)...$(NC)"
	@dotnet build $(SOLUTION_FILE) -c $(CONFIGURATION)
	@echo "$(GREEN)✓ Build complete$(NC)"

rebuild: clean build ## Clean and rebuild project

release: ## Build project in Release configuration
	@echo "$(YELLOW)Building Release configuration...$(NC)"
	@dotnet build $(SOLUTION_FILE) -c Release
	@echo "$(GREEN)✓ Release build complete$(NC)"

publish: release ## Publish as standalone executable
	@echo "$(YELLOW)Publishing release package...$(NC)"
	@dotnet publish $(PROJECT_DIR).csproj -c Release -o ./publish
	@echo "$(GREEN)✓ Published to ./publish$(NC)"

package: release ## Create NuGet package
	@echo "$(YELLOW)Creating NuGet package...$(NC)"
	@dotnet pack $(PROJECT_DIR).csproj -c Release -o ./nupkg
	@echo "$(GREEN)✓ Package created in ./nupkg$(NC)"

## Development targets
test: ## Run tests
	@echo "$(YELLOW)Running tests...$(NC)"
	@dotnet test -c Debug -v minimal
	@echo "$(GREEN)✓ Tests complete$(NC)"

analyze: build ## Run code analysis on project
	@echo "$(YELLOW)Running analysis on ./src...$(NC)"
	@dotnet run --project $(PROJECT_DIR) -c Debug -- ./src
	@echo "$(GREEN)✓ Analysis complete$(NC)"

analyze-json: build ## Run analysis and export JSON report
	@echo "$(YELLOW)Running analysis with JSON output...$(NC)"
	@dotnet run --project $(PROJECT_DIR) -c Debug -- ./src \
		--format json --output analysis.json
	@echo "$(GREEN)✓ Analysis saved to analysis.json$(NC)"

format: ## Format code using dotnet format
	@echo "$(YELLOW)Formatting code...$(NC)"
	@dotnet format $(SOLUTION_FILE)
	@echo "$(GREEN)✓ Code formatted$(NC)"

format-check: ## Check code formatting without changes
	@echo "$(YELLOW)Checking code format...$(NC)"
	@dotnet format $(SOLUTION_FILE) --verify-no-changes
	@echo "$(GREEN)✓ Code format is valid$(NC)"

lint: format-check analyze ## Run formatters and analyzers

watch: ## Watch for changes and rebuild
	@echo "$(YELLOW)Watching for changes...$(NC)"
	@dotnet watch --project $(PROJECT_DIR) run -c Debug -- ./src

## Maintenance targets
clean: ## Remove build outputs and artifacts
	@echo "$(YELLOW)Cleaning build artifacts...$(NC)"
	@dotnet clean $(SOLUTION_FILE)
	@rm -rf ./publish ./nupkg ./bin ./obj
	@rm -rf ./analysis.json ./analysis-report.html
	@echo "$(GREEN)✓ Cleaned$(NC)"

restore: ## Restore NuGet packages
	@echo "$(YELLOW)Restoring packages...$(NC)"
	@dotnet restore $(SOLUTION_FILE)
	@echo "$(GREEN)✓ Packages restored$(NC)"

deps: ## Show project dependencies
	@echo "$(BLUE)Project Dependencies:$(NC)"
	@dotnet list $(SOLUTION_FILE) package

upgrade-deps: ## Upgrade NuGet packages to latest versions
	@echo "$(YELLOW)Upgrading packages...$(NC)"
	@dotnet add $(PROJECT_DIR).csproj package Microsoft.CodeAnalysis.CSharp --version latest
	@dotnet add $(PROJECT_DIR).csproj package Microsoft.Extensions.DependencyInjection --version latest
	@dotnet add $(PROJECT_DIR).csproj package System.Collections.Immutable --version latest
	@dotnet add $(PROJECT_DIR).csproj package System.Reflection.Metadata --version latest
	@echo "$(GREEN)✓ Packages upgraded$(NC)"

## Docker targets
docker-build: ## Build Docker image
	@echo "$(YELLOW)Building Docker image...$(NC)"
	@docker build -t roslyn-guard-analyzer:latest .
	@echo "$(GREEN)✓ Docker image built$(NC)"

docker-run: ## Run analyzer in Docker container
	@echo "$(YELLOW)Running analyzer in Docker...$(NC)"
	@docker run --rm -v $$(pwd):/workspace roslyn-guard-analyzer:latest /workspace/src

docker-publish: docker-build ## Publish Docker image to registry
	@echo "$(YELLOW)Publishing Docker image...$(NC)"
	@docker tag roslyn-guard-analyzer:latest myregistry/roslyn-guard-analyzer:1.2.0
	@docker push myregistry/roslyn-guard-analyzer:1.2.0
	@echo "$(GREEN)✓ Docker image published$(NC)"

## Installation targets
install: release ## Install analyzer as global tool
	@echo "$(YELLOW)Installing analyzer...$(NC)"
	@sudo install -m 755 publish/RoslynGuardAnalyzer /usr/local/bin/roslyn-guard-analyzer
	@echo "$(GREEN)✓ Installed to /usr/local/bin/roslyn-guard-analyzer$(NC)"

uninstall: ## Uninstall analyzer
	@echo "$(YELLOW)Uninstalling analyzer...$(NC)"
	@sudo rm -f /usr/local/bin/roslyn-guard-analyzer
	@echo "$(GREEN)✓ Uninstalled$(NC)"

## Documentation targets
docs: ## Build documentation
	@echo "$(YELLOW)Generating documentation...$(NC)"
	@echo "$(GREEN)✓ Documentation is in ./docs$(NC)"

docs-serve: ## Serve documentation locally (requires Python)
	@echo "$(YELLOW)Serving documentation on http://localhost:8000...$(NC)"
	@python3 -m http.server 8000 --directory ./docs

readme: ## Display README
	@less README.md

## Utility targets
version: ## Show current version
	@grep -oP '<Version>\K[^<]+' src/RoslynGuardAnalyzer/RoslynGuardAnalyzer.csproj || echo "Unknown"

info: ## Show project information
	@echo "$(BLUE)Roslyn Guard Analyzer - Project Information$(NC)"
	@echo ""
	@echo "Project:        $$(basename $$PWD)"
	@echo "Solution:       $(SOLUTION_FILE)"
	@echo "Main Project:   $(PROJECT_DIR)"
	@echo "Version:        $$(make version)"
	@echo ".NET Target:    net10.0"
	@echo ""
	@echo "$(BLUE)Quick Commands:$(NC)"
	@echo "  make build     - Build in Debug mode"
	@echo "  make release   - Build in Release mode"
	@echo "  make test      - Run tests"
	@echo "  make analyze   - Analyze source code"
	@echo "  make clean     - Clean build artifacts"
	@echo "  make install   - Install global tool"
	@echo ""

ci: clean restore build test analyze format-check ## Full CI pipeline

all: ci package docker-build ## Build everything

.DEFAULT_GOAL := help
