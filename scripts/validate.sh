#!/bin/bash
# Validation script for Outline MCP Integration

set -e

echo "=================================="
echo "Outline MCP Integration Validator"
echo "=================================="
echo ""

# Check .NET
echo "Checking .NET SDK..."
if ! command -v dotnet &> /dev/null; then
    echo "ERROR: .NET SDK not found. Install from https://dotnet.microsoft.com/download"
    exit 1
fi

DOTNET_VERSION=$(dotnet --version)
echo "✓ .NET SDK $DOTNET_VERSION found"

# Check environment variables
echo ""
echo "Checking environment variables..."

if [ -z "$OUTLINE_BASE_URL" ]; then
    echo "ERROR: OUTLINE_BASE_URL not set"
    echo "Run: export OUTLINE_BASE_URL=\"http://localhost:3000\""
    exit 1
fi
echo "✓ OUTLINE_BASE_URL: $OUTLINE_BASE_URL"

if [ -z "$OUTLINE_API_KEY" ]; then
    echo "ERROR: OUTLINE_API_KEY not set"
    echo "Run: export OUTLINE_API_KEY=\"your-api-key\""
    exit 1
fi
echo "✓ OUTLINE_API_KEY: ${OUTLINE_API_KEY:0:10}***"

# Build project
echo ""
echo "Building project..."
dotnet build --nologo --verbosity quiet
echo "✓ Build successful"

# Run tests
echo ""
echo "Running tests..."
dotnet test --nologo --verbosity quiet --filter "FullyQualifiedName~Unit"
echo "✓ All tests passed"

echo ""
echo "=================================="
echo "Validation complete!"
echo ""
echo "Next steps:"
echo "1. List collections: dotnet run --project src/Outline.Mcp.Client -- list-collections"
echo "2. Run bootstrap: dotnet run --project src/Outline.Mcp.Client -- bootstrap --collection-id <id>"
echo "=================================="
