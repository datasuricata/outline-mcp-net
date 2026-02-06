#!/usr/bin/env pwsh
# Publish self-contained executable for Windows x64
# No .NET SDK required on target machine

param(
    [string]$Configuration = "Release"
)

Write-Host "Publishing Outline.Mcp.Server for Windows x64 (self-contained)..." -ForegroundColor Cyan

$projectPath = Join-Path (Join-Path (Join-Path $PSScriptRoot "..") "src\Outline.Mcp.Server") "Outline.Mcp.Server.csproj"
$outputPath = Join-Path (Join-Path $PSScriptRoot "..") "publish\win-x64"

# Clean previous build
if (Test-Path $outputPath) {
    Write-Host "Cleaning previous build..." -ForegroundColor Yellow
    Remove-Item $outputPath -Recurse -Force
}

# Publish self-contained single-file executable
dotnet publish $projectPath `
    --configuration $Configuration `
    --runtime win-x64 `
    --self-contained true `
    --output $outputPath `
    /p:PublishSingleFile=true `
    /p:IncludeNativeLibrariesForSelfExtract=true `
    /p:EnableCompressionInSingleFile=true `
    /p:DebugType=embedded

if ($LASTEXITCODE -eq 0) {
    Write-Host "`n[OK] Build succeeded!" -ForegroundColor Green
    
    # Copy mcp.json example
    $mcpExampleSource = Join-Path (Join-Path $PSScriptRoot "..") "mcp.executable.json.example"
    $mcpExampleDest = Join-Path $outputPath "mcp.json.example"
    if (Test-Path $mcpExampleSource) {
        Copy-Item $mcpExampleSource $mcpExampleDest -Force
        Write-Host "  [OK] Copied mcp.json.example" -ForegroundColor Gray
    }
    
    Write-Host "`nExecutable location:" -ForegroundColor Cyan
    Write-Host "  $outputPath\Outline.Mcp.Server.exe" -ForegroundColor White
    
    $exePath = Join-Path $outputPath "Outline.Mcp.Server.exe"
    if (Test-Path $exePath) {
        $size = (Get-Item $exePath).Length / 1MB
        Write-Host "`nFile size: $([math]::Round($size, 2)) MB" -ForegroundColor Gray
    }
    
    Write-Host "`nUsage:" -ForegroundColor Cyan
    Write-Host "  Set environment variables:" -ForegroundColor Gray
    Write-Host "    `$env:OUTLINE_API_KEY = 'your-api-key'" -ForegroundColor White
    Write-Host "    `$env:OUTLINE_BASE_URL = 'http://localhost:3000'" -ForegroundColor White
    Write-Host "  Then run:" -ForegroundColor Gray
    Write-Host "    .\publish\win-x64\Outline.Mcp.Server.exe" -ForegroundColor White
}
else {
    Write-Host "`n[FAIL] Build failed!" -ForegroundColor Red
    exit 1
}
