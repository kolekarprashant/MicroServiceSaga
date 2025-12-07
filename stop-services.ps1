# Stop All Running Services

Write-Host "Stopping all microservices..." -ForegroundColor Yellow

# Find and stop all dotnet processes running our services
Get-Process | Where-Object {
    $_.ProcessName -eq "dotnet" -and 
    ($_.MainWindowTitle -like "*OrderService*" -or 
     $_.MainWindowTitle -like "*PaymentService*" -or 
     $_.MainWindowTitle -like "*Saga.Api*")
} | Stop-Process -Force

Write-Host "✓ All services stopped" -ForegroundColor Green
