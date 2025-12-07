# Start All Microservices

Write-Host "╔══════════════════════════════════════════════════════════════════════════════╗" -ForegroundColor Cyan
Write-Host "║          Starting Saga Orchestration Microservices                           ║" -ForegroundColor Cyan
Write-Host "╚══════════════════════════════════════════════════════════════════════════════╝" -ForegroundColor Cyan
Write-Host ""

# Build all projects
Write-Host "Building all projects..." -ForegroundColor Yellow
dotnet build --nologo -v q

if ($LASTEXITCODE -ne 0) {
    Write-Host "✗ Build failed!" -ForegroundColor Red
    exit 1
}

Write-Host "✓ Build successful!" -ForegroundColor Green
Write-Host ""

# Start services in background
Write-Host "Starting services..." -ForegroundColor Yellow
Write-Host ""

# Start Order Service (Port 5001)
Write-Host "→ Starting Order Service on http://localhost:5001" -ForegroundColor Cyan
Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd '$PWD'; dotnet run --project OrderService.Api\OrderService.Api.csproj --no-build"
Start-Sleep -Seconds 2

# Start Payment Service (Port 5002)
Write-Host "→ Starting Payment Service on http://localhost:5002" -ForegroundColor Cyan
Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd '$PWD'; dotnet run --project PaymentService.Api\PaymentService.Api.csproj --no-build"
Start-Sleep -Seconds 2

# Start Saga API Gateway (Port 5000)
Write-Host "→ Starting Saga API Gateway on http://localhost:5000" -ForegroundColor Cyan
Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd '$PWD'; dotnet run --project Saga.Api\Saga.Api.csproj --no-build"
Start-Sleep -Seconds 3

Write-Host ""
Write-Host "╔══════════════════════════════════════════════════════════════════════════════╗" -ForegroundColor Green
Write-Host "║                    All Services Started Successfully!                        ║" -ForegroundColor Green
Write-Host "╚══════════════════════════════════════════════════════════════════════════════╝" -ForegroundColor Green
Write-Host ""

Write-Host "Services running on:" -ForegroundColor White
Write-Host "  • Saga API Gateway:  http://localhost:5000/swagger" -ForegroundColor Yellow
Write-Host "  • Order Service:     http://localhost:5001/swagger" -ForegroundColor Yellow
Write-Host "  • Payment Service:   http://localhost:5002/swagger" -ForegroundColor Yellow
Write-Host ""

Write-Host "To execute a saga transaction, POST to:" -ForegroundColor White
Write-Host "  http://localhost:5000/api/saga/execute" -ForegroundColor Cyan
Write-Host ""

Write-Host "Sample request:" -ForegroundColor White
Write-Host @"
{
  "customerId": "CUST001",
  "productId": "PROD001",
  "quantity": 2,
  "amount": 2000
}
"@ -ForegroundColor Gray

Write-Host ""
Write-Host "Press Ctrl+C in each terminal window to stop services" -ForegroundColor Yellow
