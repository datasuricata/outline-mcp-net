# Validation script for Outline MCP Integration (PowerShell)

$ErrorActionPreference = "Stop"

Write-Host "==================================" -ForegroundColor Yellow
Write-Host "Outline MCP Integration Validator" -ForegroundColor Yellow
Write-Host "==================================" -ForegroundColor Yellow
Write-Host ""

# Check .NET
Write-Host "Checking .NET SDK..." -ForegroundColor Cyan
try {
    $dotnetVersion = dotnet --version
    Write-Host "✓ .NET SDK $dotnetVersion found" -ForegroundColor Green
} catch {
    Write-Host "ERROR: .NET SDK not found. Install from https://dotnet.microsoft.com/download" -ForegroundColor Red
    exit 1
}

# Check environment variables
Write-Host ""
Write-Host "Checking environment variables..." -ForegroundColor Cyan

if ([string]::IsNullOrEmpty($env:OUTLINE_BASE_URL)) {
    Write-Host "ERROR: OUTLINE_BASE_URL not set" -ForegroundColor Red
    Write-Host "Run: `$env:OUTLINE_BASE_URL=`"http://localhost:3000`"" -ForegroundColor Yellow
    exit 1
}
Write-Host "✓ OUTLINE_BASE_URL: $env:OUTLINE_BASE_URL" -ForegroundColor Green

if ([string]::IsNullOrEmpty($env:OUTLINE_API_KEY)) {
    Write-Host "ERROR: OUTLINE_API_KEY not set" -ForegroundColor Red
    Write-Host "Run: `$env:OUTLINE_API_KEY=`"your-api-key`"" -ForegroundColor Yellow
    exit 1
}
$maskedKey = $env:OUTLINE_API_KEY.Substring(0, [Math]::Min(10, $env:OUTLINE_API_KEY.Length)) + "***"
Write-Host "✓ OUTLINE_API_KEY: $maskedKey" -ForegroundColor Green

# Build project
Write-Host ""
Write-Host "Building project..." -ForegroundColor Cyan
dotnet build --nologo --verbosity quiet
if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: Build failed" -ForegroundColor Red
    exit 1
}
Write-Host "✓ Build successful" -ForegroundColor Green

# Run tests
Write-Host ""
Write-Host "Running tests..." -ForegroundColor Cyan
dotnet test --nologo --verbosity quiet --filter "FullyQualifiedName~Unit"
if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: Tests failed" -ForegroundColor Red
    exit 1
}
Write-Host "✓ All tests passed" -ForegroundColor Green

Write-Host ""
Write-Host "==================================" -ForegroundColor Green
Write-Host "Validation complete!" -ForegroundColor Green
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Cyan
Write-Host "1. List collections: dotnet run --project src/Outline.Mcp.Client -- list-collections"
Write-Host "2. Run bootstrap: dotnet run --project src/Outline.Mcp.Client -- bootstrap --collection-id <id>"
Write-Host "==================================" -ForegroundColor Green
