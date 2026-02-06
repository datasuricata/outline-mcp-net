#!/bin/bash
# Publish self-contained executable for macOS ARM64 (Apple Silicon)
# No .NET SDK required on target machine

set -e

CONFIGURATION="${1:-Release}"
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_PATH="$SCRIPT_DIR/../src/Outline.Mcp.Server/Outline.Mcp.Server.csproj"
OUTPUT_PATH="$SCRIPT_DIR/../publish/osx-arm64"

echo -e "\033[36mPublishing Outline.Mcp.Server for macOS ARM64 (self-contained)...\033[0m"

# Clean previous build
if [ -d "$OUTPUT_PATH" ]; then
    echo -e "\033[33mCleaning previous build...\033[0m"
    rm -rf "$OUTPUT_PATH"
fi

# Publish self-contained single-file executable
dotnet publish "$PROJECT_PATH" \
    --configuration "$CONFIGURATION" \
    --runtime osx-arm64 \
    --self-contained true \
    --output "$OUTPUT_PATH" \
    /p:PublishSingleFile=true \
    /p:IncludeNativeLibrariesForSelfExtract=true \
    /p:EnableCompressionInSingleFile=true \
    /p:DebugType=embedded

if [ $? -eq 0 ]; then
    echo -e "\n\033[32m[OK] Build succeeded!\033[0m"
    
    # Copy mcp.json example
    MCP_EXAMPLE_SOURCE="$SCRIPT_DIR/../mcp.executable.json.example"
    MCP_EXAMPLE_DEST="$OUTPUT_PATH/mcp.json.example"
    if [ -f "$MCP_EXAMPLE_SOURCE" ]; then
        cp "$MCP_EXAMPLE_SOURCE" "$MCP_EXAMPLE_DEST"
        echo -e "  \033[90m[OK] Copied mcp.json.example\033[0m"
    fi
    
    echo -e "\n\033[36mExecutable location:\033[0m"
    echo -e "  $OUTPUT_PATH/Outline.Mcp.Server"
    
    if [ -f "$OUTPUT_PATH/Outline.Mcp.Server" ]; then
        SIZE=$(du -h "$OUTPUT_PATH/Outline.Mcp.Server" | cut -f1)
        echo -e "\n\033[90mFile size: $SIZE\033[0m"
        chmod +x "$OUTPUT_PATH/Outline.Mcp.Server"
    fi
    
    echo -e "\n\033[36mUsage:\033[0m"
    echo -e "\033[90m  Set environment variables:\033[0m"
    echo -e "\033[37m    export OUTLINE_API_KEY='your-api-key'\033[0m"
    echo -e "\033[37m    export OUTLINE_BASE_URL='http://localhost:3000'\033[0m"
    echo -e "\033[90m  Then run:\033[0m"
    echo -e "\033[37m    ./publish/osx-arm64/Outline.Mcp.Server\033[0m"
else
    echo -e "\n\033[31m[FAIL] Build failed!\033[0m"
    exit 1
fi
