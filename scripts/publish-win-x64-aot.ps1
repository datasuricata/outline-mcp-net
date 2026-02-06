#!/usr/bin/env pwsh
# Publish Native AOT executable for Windows x64
# Smallest size, fastest startup, no .NET runtime required

param(
    [string]$Configuration = "Release"
)

Write-Host "Publishing Outline.Mcp.Server for Windows x64 (Native AOT)..." -ForegroundColor Cyan

$projectPath = Join-Path (Join-Path (Join-Path $PSScriptRoot "..") "src\Outline.Mcp.Server") "Outline.Mcp.Server.csproj"
$outputPath = Join-Path (Join-Path $PSScriptRoot "..") "publish\win-x64-aot"

# Clean previous build
if (Test-Path $outputPath) {
    Write-Host "Cleaning previous build..." -ForegroundColor Yellow
    Remove-Item $outputPath -Recurse -Force
}

# Publish Native AOT
dotnet publish $projectPath `
    --configuration $Configuration `
    --runtime win-x64 `
    --output $outputPath `
    /p:PublishAot=true `
    /p:InvariantGlobalization=true `
    /p:StripSymbols=true

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
        Write-Host "Native AOT - Fastest startup, smallest size!" -ForegroundColor Green
    }
    
    Write-Host "`nUsage:" -ForegroundColor Cyan
    Write-Host "  Set environment variables:" -ForegroundColor Gray
    Write-Host "    `$env:OUTLINE_API_KEY = 'your-api-key'" -ForegroundColor White
    Write-Host "    `$env:OUTLINE_BASE_URL = 'http://localhost:3000'" -ForegroundColor White
    Write-Host "  Then run:" -ForegroundColor Gray
    Write-Host "    .\publish\win-x64-aot\Outline.Mcp.Server.exe" -ForegroundColor White
}
else {
    Write-Host "`n[FAIL] Build failed!" -ForegroundColor Red
    Write-Host "`nNote: Native AOT requires compatible code. If build fails:" -ForegroundColor Yellow
    Write-Host "  - Use standard self-contained build instead (publish-win-x64.ps1)" -ForegroundColor Yellow
    Write-Host "  - Check for reflection or dynamic code that needs AOT compatibility" -ForegroundColor Yellow
    exit 1
}
