# Test Saga API with sample requests

$baseUrl = "http://localhost:5000"

Write-Host "╔══════════════════════════════════════════════════════════════════════════════╗" -ForegroundColor Cyan
Write-Host "║                    Testing Saga Orchestration API                            ║" -ForegroundColor Cyan
Write-Host "╚══════════════════════════════════════════════════════════════════════════════╝" -ForegroundColor Cyan
Write-Host ""

# Test 1: Successful transaction
Write-Host "TEST 1: Successful Transaction" -ForegroundColor Yellow
Write-Host "-------------------------------" -ForegroundColor Yellow

$request1 = @{
    customerId = "CUST001"
    productId = "PROD001"
    quantity = 2
    amount = 2000
} | ConvertTo-Json

try {
    $response1 = Invoke-RestMethod -Uri "$baseUrl/api/saga/execute" -Method Post -Body $request1 -ContentType "application/json"
    Write-Host "✓ Transaction completed successfully!" -ForegroundColor Green
    Write-Host "Transaction ID: $($response1.transactionId)" -ForegroundColor Cyan
    Write-Host "Order ID: $($response1.orderId)" -ForegroundColor Cyan
    Write-Host "State: $($response1.state)" -ForegroundColor Green
    Write-Host ""
} catch {
    Write-Host "✗ Request failed: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host ""
}

Start-Sleep -Seconds 2

# Test 2: Insufficient funds
Write-Host "TEST 2: Payment Failure (Insufficient Funds)" -ForegroundColor Yellow
Write-Host "---------------------------------------------" -ForegroundColor Yellow

$request2 = @{
    customerId = "CUST002"
    productId = "PROD001"
    quantity = 1
    amount = 1500
} | ConvertTo-Json

try {
    $response2 = Invoke-RestMethod -Uri "$baseUrl/api/saga/execute" -Method Post -Body $request2 -ContentType "application/json"
} catch {
    $errorResponse = $_.ErrorDetails.Message | ConvertFrom-Json
    Write-Host "✗ Transaction failed as expected" -ForegroundColor Yellow
    Write-Host "Transaction ID: $($errorResponse.transactionId)" -ForegroundColor Cyan
    Write-Host "State: $($errorResponse.state)" -ForegroundColor Red
    Write-Host "Error: $($errorResponse.errorMessage)" -ForegroundColor Red
    Write-Host "Compensated Steps: $($errorResponse.compensatedSteps -join ', ')" -ForegroundColor Yellow
    Write-Host ""
}

Start-Sleep -Seconds 2

# Get all transactions
Write-Host "Fetching All Transactions..." -ForegroundColor Yellow
Write-Host "----------------------------" -ForegroundColor Yellow

try {
    $allTransactions = Invoke-RestMethod -Uri "$baseUrl/api/saga" -Method Get
    Write-Host "Total transactions: $($allTransactions.Count)" -ForegroundColor Cyan
    foreach ($txn in $allTransactions) {
        Write-Host "  • $($txn.transactionId): $($txn.state)" -ForegroundColor Gray
    }
    Write-Host ""
} catch {
    Write-Host "✗ Failed to fetch transactions: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host ""
}

Write-Host "╔══════════════════════════════════════════════════════════════════════════════╗" -ForegroundColor Green
Write-Host "║                         API Testing Complete                                 ║" -ForegroundColor Green
Write-Host "╚══════════════════════════════════════════════════════════════════════════════╝" -ForegroundColor Green
