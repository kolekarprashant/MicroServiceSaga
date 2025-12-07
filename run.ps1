# Saga Pattern Demo - Quick Run Script

Write-Host "╔══════════════════════════════════════════════════════════════════════════════╗" -ForegroundColor Cyan
Write-Host "║           Building Saga Orchestration Pattern Project...                    ║" -ForegroundColor Cyan
Write-Host "╚══════════════════════════════════════════════════════════════════════════════╝" -ForegroundColor Cyan
Write-Host ""

# Build the project
dotnet build --nologo -v q

if ($LASTEXITCODE -eq 0) {
    Write-Host "✓ Build successful!" -ForegroundColor Green
    Write-Host ""
    Write-Host "╔══════════════════════════════════════════════════════════════════════════════╗" -ForegroundColor Cyan
    Write-Host "║                      Running Saga Demo...                                   ║" -ForegroundColor Cyan
    Write-Host "╚══════════════════════════════════════════════════════════════════════════════╝" -ForegroundColor Cyan
    Write-Host ""
    
    # Run the project
    dotnet run --project SagaPattern\SagaPattern.csproj --no-build
} else {
    Write-Host "✗ Build failed!" -ForegroundColor Red
}
