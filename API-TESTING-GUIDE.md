# Web API Testing Guide

## 🚀 Quick Start

### 1. Start All Services
```powershell
.\run-all-services.ps1
```

This starts 4 Web APIs:
- **Saga Orchestrator**: http://localhost:5000
- **Order Service**: http://localhost:5001
- **Payment Service**: http://localhost:5002
- **Inventory Service**: http://localhost:5003

### 2. Access Swagger UI
Open your browser and navigate to:
- http://localhost:5001/swagger (Order Service - Start here!)
- http://localhost:5002/swagger (Payment Service)
- http://localhost:5003/swagger (Inventory Service)
- http://localhost:5000/swagger (Saga Orchestrator)

## 📝 API Endpoints

### Order Service (Port 5001)

#### Submit a New Order
```bash
POST /api/orders
Content-Type: application/json

{
  "amount": 150.00,
  "productId": "PROD-1234",
  "quantity": 2
}
```

**Using curl:**
```bash
curl -X POST http://localhost:5001/api/orders -H "Content-Type: application/json" -d "{\"amount\":150.00,\"productId\":\"PROD-1234\",\"quantity\":2}"
```

**Using PowerShell:**
```powershell
$body = @{
    amount = 150.00
    productId = "PROD-1234"
    quantity = 2
} | ConvertTo-Json

Invoke-RestMethod -Uri "http://localhost:5001/api/orders" -Method POST -Body $body -ContentType "application/json"
```

#### Get Order Status
```bash
GET /api/orders/{orderId}
```

#### Get All Orders
```bash
GET /api/orders
```

### Payment Service (Port 5002)

#### Get Payment Status
```bash
GET /api/payments/{orderId}
```

#### Get All Payments
```bash
GET /api/payments
```

#### Get Payment Statistics
```bash
GET /api/payments/stats
```

### Inventory Service (Port 5003)

#### Get Inventory by Order
```bash
GET /api/inventory/order/{orderId}
```

#### Get All Inventory Records
```bash
GET /api/inventory
```

#### Get Inventory by Product
```bash
GET /api/inventory/product/{productId}
```

#### Get Inventory Statistics
```bash
GET /api/inventory/stats
```

### Saga Orchestrator (Port 5000)

#### Health Check
```bash
GET /api/saga/health
```

#### Get Saga Info
```bash
GET /api/saga/info
```

## 🧪 Complete Test Workflow

### Test 1: Successful Order Flow

1. **Submit Order**
```bash
curl -X POST http://localhost:5001/api/orders -H "Content-Type: application/json" -d "{\"amount\":120.00,\"productId\":\"PROD-5678\",\"quantity\":3}"
```

2. **Copy the `orderId` from the response**

3. **Check Order Status** (wait 2 seconds)
```bash
curl http://localhost:5001/api/orders/{orderId}
```

4. **Check Payment Status**
```bash
curl http://localhost:5002/api/payments/{orderId}
```

5. **Check Inventory Status**
```bash
curl http://localhost:5003/api/inventory/order/{orderId}
```

### Test 2: Failed Order with Compensation

1. **Submit High-Value Order** (higher chance of failure)
```bash
curl -X POST http://localhost:5001/api/orders -H "Content-Type: application/json" -d "{\"amount\":450.00,\"productId\":\"PROD-9999\",\"quantity\":5}"
```

2. **Watch for compensation** - Inventory will be released if payment fails

3. **Check All Services**
```bash
# Order status should be "Failed"
curl http://localhost:5001/api/orders/{orderId}

# Payment status should be "Failed" with a reason
curl http://localhost:5002/api/payments/{orderId}

# Inventory status should be "Released" (compensation)
curl http://localhost:5003/api/inventory/order/{orderId}
```

## 📊 View Statistics

### Payment Statistics
```bash
curl http://localhost:5002/api/payments/stats
```

**Response:**
```json
{
  "totalPayments": 10,
  "successfulPayments": 7,
  "failedPayments": 3,
  "successRate": 70.00,
  "totalAmountProcessed": 1245.50
}
```

### Inventory Statistics
```bash
curl http://localhost:5003/api/inventory/stats
```

**Response:**
```json
{
  "totalRecords": 10,
  "currentlyReserved": 7,
  "released": 3,
  "products": [
    { "productId": "PROD-1234", "count": 5 },
    { "productId": "PROD-5678", "count": 3 }
  ]
}
```

### All Orders
```bash
curl http://localhost:5001/api/orders
```

## 🎯 Understanding the Saga Flow

### Successful Flow:
```
1. POST /api/orders → Order Service
2. SubmitOrder event → Saga Orchestrator
3. ReserveInventory command → Inventory Service
4. InventoryReserved event → Saga Orchestrator
5. ProcessPayment command → Payment Service
6. PaymentProcessed event → Saga Orchestrator
7. OrderCompleted event → Order Service
8. Order Status: "Completed" ✅
```

### Failed Flow with Compensation:
```
1. POST /api/orders → Order Service
2. SubmitOrder event → Saga Orchestrator
3. ReserveInventory command → Inventory Service
4. InventoryReserved event → Saga Orchestrator
5. ProcessPayment command → Payment Service
6. PaymentFailed event → Saga Orchestrator
7. ReleaseInventory command → Inventory Service (Compensation)
8. InventoryReleased event → Saga Orchestrator
9. OrderFailed event → Order Service
10. Order Status: "Failed", Inventory Status: "Released" ❌
```

## 🔧 PowerShell Helper Functions

```powershell
# Submit Order Function
function Submit-Order {
    param(
        [decimal]$Amount,
        [string]$ProductId,
        [int]$Quantity
    )
    
    $body = @{
        amount = $Amount
        productId = $ProductId
        quantity = $Quantity
    } | ConvertTo-Json
    
    Invoke-RestMethod -Uri "http://localhost:5001/api/orders" -Method POST -Body $body -ContentType "application/json"
}

# Get Order Status Function
function Get-OrderStatus {
    param([string]$OrderId)
    Invoke-RestMethod -Uri "http://localhost:5001/api/orders/$OrderId"
}

# Usage:
# $order = Submit-Order -Amount 150.00 -ProductId "PROD-1234" -Quantity 2
# Start-Sleep -Seconds 2
# Get-OrderStatus -OrderId $order.orderId
```

## 🐛 Troubleshooting

**Services not communicating?**
- Ensure all 4 services are running
- Check console output for errors
- Verify ports are not in use

**Order stuck in "Submitted" status?**
- Check that Saga Orchestrator is running
- Check Inventory and Payment services are running
- Review console logs for errors

**Swagger not loading?**
- Wait 5-10 seconds after service starts
- Refresh the browser
- Check the service is running on the correct port

## 📖 Additional Resources

- Use Swagger UI for interactive testing
- Check console logs for real-time saga orchestration
- Monitor all 4 service windows to see message flow
- Orders typically complete within 1-2 seconds

---

**Happy Testing!** 🎉
