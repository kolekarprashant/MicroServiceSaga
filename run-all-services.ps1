# PowerShell script to run all Web API services in separate windows

Write-Host "🚀 Starting all MassTransit Saga Demo Web APIs..." -ForegroundColor Green
Write-Host ""

# Get the solution directory
$solutionDir = Split-Path -Parent $MyInvocation.MyCommand.Path

# Start each service in a new PowerShell window
Write-Host "Starting Saga Orchestrator API (Port 5000)..." -ForegroundColor Cyan
Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd '$solutionDir'; Write-Host '🎯 SAGA ORCHESTRATOR API - http://localhost:5000' -ForegroundColor Yellow; dotnet run --project src\MassTransitSagaDemo\MassTransitSagaDemo.csproj"

Start-Sleep -Seconds 3

Write-Host "Starting Order Service API (Port 5001)..." -ForegroundColor Cyan
Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd '$solutionDir'; Write-Host '🛒 ORDER SERVICE API - http://localhost:5001' -ForegroundColor Yellow; dotnet run --project src\OrderService\OrderService.csproj"

Start-Sleep -Seconds 3

Write-Host "Starting Payment Service API (Port 5002)..." -ForegroundColor Cyan
Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd '$solutionDir'; Write-Host '💳 PAYMENT SERVICE API - http://localhost:5002' -ForegroundColor Yellow; dotnet run --project src\PaymentService\PaymentService.csproj"

Start-Sleep -Seconds 3

Write-Host "Starting Inventory Service API (Port 5003)..." -ForegroundColor Cyan
Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd '$solutionDir'; Write-Host '📦 INVENTORY SERVICE API - http://localhost:5003' -ForegroundColor Yellow; dotnet run --project src\InventoryService\InventoryService.csproj"

Write-Host ""
Write-Host "✅ All Web API services started!" -ForegroundColor Green
Write-Host ""
Write-Host "📍 Service URLs:" -ForegroundColor Cyan
Write-Host "   Saga Orchestrator: http://localhost:5000/swagger" -ForegroundColor White
Write-Host "   Order Service:     http://localhost:5001/swagger" -ForegroundColor White
Write-Host "   Payment Service:   http://localhost:5002/swagger" -ForegroundColor White
Write-Host "   Inventory Service: http://localhost:5003/swagger" -ForegroundColor White
Write-Host ""
Write-Host "🧪 Test with:" -ForegroundColor Cyan
Write-Host "   curl -X POST http://localhost:5001/api/orders -H 'Content-Type: application/json' -d '{\"amount\":150.00,\"productId\":\"PROD-1234\",\"quantity\":2}'" -ForegroundColor White
Write-Host ""
Write-Host "   Press Ctrl+C in each window to stop the services." -ForegroundColor Gray
Write-Host ""
