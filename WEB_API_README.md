# 🌐 WEB API - QUICK START GUIDE

## ✨ What's New

The Saga Orchestration Pattern now includes **three RESTful Web API microservices**:

### 🎯 Services
1. **Saga API Gateway** (Port 5000) - Orchestrates distributed transactions
2. **Order Service** (Port 5001) - Manages orders
3. **Payment Service** (Port 5002) - Processes payments

---

## 🚀 Quick Start (3 Steps)

### Step 1: Start All Services
```powershell
.\start-services.ps1
```

This launches all three APIs in separate terminal windows.

### Step 2: Access Swagger UI
Open your browser:
- **Saga Gateway**: http://localhost:5000/swagger
- **Order Service**: http://localhost:5001/swagger
- **Payment Service**: http://localhost:5002/swagger

### Step 3: Test the API
```powershell
.\test-api.ps1
```

This runs automated tests against the API.

---

## 📡 Execute a Saga Transaction

### Using PowerShell
```powershell
$body = @{
    customerId = "CUST001"
    productId = "PROD001"
    quantity = 2
    amount = 2000
} | ConvertTo-Json

Invoke-RestMethod -Uri "http://localhost:5000/api/saga/execute" `
    -Method Post `
    -Body $body `
    -ContentType "application/json"
```

### Using curl
```bash
curl -X POST http://localhost:5000/api/saga/execute \
  -H "Content-Type: application/json" \
  -d '{
    "customerId": "CUST001",
    "productId": "PROD001",
    "quantity": 2,
    "amount": 2000
  }'
```

### Using Swagger UI
1. Navigate to http://localhost:5000/swagger
2. Click on `POST /api/saga/execute`
3. Click "Try it out"
4. Enter request body
5. Click "Execute"

---

## 🎬 Demo Scenarios

### ✅ Scenario 1: Successful Transaction
```json
{
  "customerId": "CUST001",
  "productId": "PROD001",
  "quantity": 2,
  "amount": 2000
}
```
**Result**: Transaction completes successfully

### ❌ Scenario 2: Payment Failure
```json
{
  "customerId": "CUST002",
  "productId": "PROD001",
  "quantity": 1,
  "amount": 1500
}
```
**Result**: Payment fails (insufficient funds), order is automatically cancelled

---

## 📊 Test Data

### Customer Balances
| Customer ID | Balance  |
|-------------|----------|
| CUST001     | $10,000  |
| CUST002     | $50      |
| CUST003     | $5,000   |
| CUST004     | $20,000  |

---

## 🛑 Stop Services
```powershell
.\stop-services.ps1
```

Or press `Ctrl+C` in each terminal window.

---

## 📚 Full Documentation
See [API_DOCUMENTATION.md](API_DOCUMENTATION.md) for complete API reference.

---

## 🎯 Key Endpoints

### Saga Gateway (http://localhost:5000)
- `POST /api/saga/execute` - Execute saga transaction
- `GET /api/saga` - Get all transactions
- `GET /api/saga/{id}` - Get transaction by ID

### Order Service (http://localhost:5001)
- `POST /api/orders` - Create order
- `GET /api/orders/{id}` - Get order
- `PUT /api/orders/{id}/confirm` - Confirm order
- `PUT /api/orders/{id}/cancel` - Cancel order

### Payment Service (http://localhost:5002)
- `POST /api/payments` - Process payment
- `GET /api/payments/{id}` - Get payment
- `POST /api/payments/{id}/refund` - Refund payment
- `GET /api/payments/customers/{id}/balance` - Get balance

---

## ✨ Features

✅ **RESTful APIs** - Standard HTTP endpoints  
✅ **Swagger Documentation** - Interactive API explorer  
✅ **Distributed Saga** - Cross-service transactions  
✅ **Automatic Compensation** - Rollback on failures  
✅ **Async Communication** - HttpClient-based  
✅ **Comprehensive Logging** - Detailed traces  

---

## 🔧 Troubleshooting

### Port Already in Use
```powershell
# Check ports
netstat -ano | findstr "5000 5001 5002"

# Kill process
Stop-Process -Id <PID> -Force
```

### Services Won't Start
- Run as Administrator
- Check firewall settings
- Ensure .NET 8/9 SDK is installed

---

**For complete API documentation, see [API_DOCUMENTATION.md](API_DOCUMENTATION.md)**
