# 🌐 WEB API DOCUMENTATION

## Overview

This implementation provides three microservices that work together using the Saga Orchestration Pattern:

1. **Saga API Gateway** (Port 5000) - Orchestrates distributed transactions
2. **Order Service** (Port 5001) - Manages orders
3. **Payment Service** (Port 5002) - Processes payments

---

## 🚀 Quick Start

### Start All Services

```powershell
.\start-services.ps1
```

This will start all three services:
- Saga API Gateway: http://localhost:5000/swagger
- Order Service: http://localhost:5001/swagger
- Payment Service: http://localhost:5002/swagger

### Stop All Services

```powershell
.\stop-services.ps1
```

### Test the API

```powershell
.\test-api.ps1
```

---

## 📡 API Endpoints

### Saga API Gateway (Port 5000)

#### Execute Saga Transaction
**POST** `/api/saga/execute`

Executes a complete saga transaction (create order → process payment → confirm order).

**Request Body:**
```json
{
  "customerId": "CUST001",
  "productId": "PROD001",
  "quantity": 2,
  "amount": 2000
}
```

**Response (Success):**
```json
{
  "transactionId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "orderId": "550e8400-e29b-41d4-a716-446655440000",
  "paymentId": "9a63c2c0-95c2-4c31-8f9f-4d1c5e5c2a35",
  "state": "Completed",
  "startedAt": "2025-11-26T10:30:00Z",
  "completedAt": "2025-11-26T10:30:02Z",
  "executedSteps": ["OrderCreated", "PaymentProcessed"],
  "compensatedSteps": [],
  "errorMessage": null,
  "durationSeconds": 2.15
}
```

**Response (Failure with Compensation):**
```json
{
  "transactionId": "7c9e6679-7425-40de-944b-e07fc1f90ae7",
  "orderId": "a1b2c3d4-e5f6-4a5b-8c7d-8e9f0a1b2c3d",
  "paymentId": null,
  "state": "Compensated",
  "startedAt": "2025-11-26T10:35:00Z",
  "completedAt": "2025-11-26T10:35:01Z",
  "executedSteps": ["OrderCreated"],
  "compensatedSteps": ["OrderCancelled"],
  "errorMessage": "Payment failed: Insufficient funds. Balance: $50, Required: $1500",
  "durationSeconds": 1.25
}
```

**Status Codes:**
- `200 OK` - Transaction completed successfully
- `400 Bad Request` - Transaction failed (with compensation)
- `500 Internal Server Error` - Unexpected error

---

#### Get Transaction by ID
**GET** `/api/saga/{transactionId}`

Retrieves details of a specific saga transaction.

**Example:**
```
GET http://localhost:5000/api/saga/3fa85f64-5717-4562-b3fc-2c963f66afa6
```

**Status Codes:**
- `200 OK` - Transaction found
- `404 Not Found` - Transaction doesn't exist

---

#### Get All Transactions
**GET** `/api/saga`

Retrieves all saga transactions.

**Example:**
```
GET http://localhost:5000/api/saga
```

**Response:**
```json
[
  {
    "transactionId": "...",
    "state": "Completed",
    ...
  },
  {
    "transactionId": "...",
    "state": "Compensated",
    ...
  }
]
```

---

### Order Service (Port 5001)

#### Create Order
**POST** `/api/orders`

Creates a new order.

**Request Body:**
```json
{
  "customerId": "CUST001",
  "productId": "PROD001",
  "quantity": 2,
  "amount": 2000
}
```

**Response:**
```json
{
  "orderId": "550e8400-e29b-41d4-a716-446655440000",
  "customerId": "CUST001",
  "productId": "PROD001",
  "quantity": 2,
  "amount": 2000,
  "status": "Pending",
  "createdAt": "2025-11-26T10:30:00Z",
  "updatedAt": null
}
```

---

#### Get Order by ID
**GET** `/api/orders/{orderId}`

**Example:**
```
GET http://localhost:5001/api/orders/550e8400-e29b-41d4-a716-446655440000
```

---

#### Get All Orders
**GET** `/api/orders`

Optional query parameter: `customerId`

**Examples:**
```
GET http://localhost:5001/api/orders
GET http://localhost:5001/api/orders?customerId=CUST001
```

---

#### Confirm Order
**PUT** `/api/orders/{orderId}/confirm`

Confirms an order (marks it as Confirmed).

**Example:**
```
PUT http://localhost:5001/api/orders/550e8400-e29b-41d4-a716-446655440000/confirm
```

---

#### Cancel Order
**PUT** `/api/orders/{orderId}/cancel`

Cancels an order.

**Request Body:**
```json
{
  "orderId": "550e8400-e29b-41d4-a716-446655440000",
  "reason": "Payment failed"
}
```

---

#### Mark Order as Failed
**PUT** `/api/orders/{orderId}/fail`

Marks an order as failed.

**Request Body:**
```json
{
  "orderId": "550e8400-e29b-41d4-a716-446655440000",
  "reason": "System error"
}
```

---

### Payment Service (Port 5002)

#### Process Payment
**POST** `/api/payments`

Processes a payment for an order.

**Request Body:**
```json
{
  "orderId": "550e8400-e29b-41d4-a716-446655440000",
  "customerId": "CUST001",
  "amount": 2000
}
```

**Response (Success):**
```json
{
  "paymentId": "9a63c2c0-95c2-4c31-8f9f-4d1c5e5c2a35",
  "orderId": "550e8400-e29b-41d4-a716-446655440000",
  "customerId": "CUST001",
  "amount": 2000,
  "status": "Success",
  "processedAt": "2025-11-26T10:30:01Z",
  "failureReason": null
}
```

**Response (Failure):**
```json
{
  "paymentId": "a1b2c3d4-e5f6-4a5b-8c7d-8e9f0a1b2c3d",
  "orderId": "...",
  "customerId": "CUST002",
  "amount": 1500,
  "status": "Failed",
  "processedAt": "2025-11-26T10:35:01Z",
  "failureReason": "Insufficient funds. Balance: $50, Required: $1500"
}
```

**Status Codes:**
- `201 Created` - Payment successful
- `400 Bad Request` - Payment failed
- `500 Internal Server Error` - Unexpected error

---

#### Get Payment by ID
**GET** `/api/payments/{paymentId}`

---

#### Get All Payments
**GET** `/api/payments`

Optional query parameter: `customerId`

---

#### Refund Payment
**POST** `/api/payments/{paymentId}/refund`

Refunds a successful payment.

**Example:**
```
POST http://localhost:5002/api/payments/9a63c2c0-95c2-4c31-8f9f-4d1c5e5c2a35/refund
```

---

#### Get Customer Balance
**GET** `/api/payments/customers/{customerId}/balance`

Retrieves a customer's current balance.

**Example:**
```
GET http://localhost:5002/api/payments/customers/CUST001/balance
```

**Response:**
```json
{
  "customerId": "CUST001",
  "balance": 8000
}
```

---

## 📊 Test Data

### Customers
| Customer ID | Initial Balance |
|-------------|-----------------|
| CUST001     | $10,000        |
| CUST002     | $50            |
| CUST003     | $5,000         |
| CUST004     | $20,000        |

### Products (for reference)
| Product ID | Name     |
|------------|----------|
| PROD001    | Laptop   |
| PROD002    | Mouse    |
| PROD003    | Keyboard |

---

## 🧪 Testing Scenarios

### Scenario 1: Successful Transaction
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

**Expected:** Transaction completes successfully.

---

### Scenario 2: Payment Failure (Insufficient Funds)
```bash
curl -X POST http://localhost:5000/api/saga/execute \
  -H "Content-Type: application/json" \
  -d '{
    "customerId": "CUST002",
    "productId": "PROD001",
    "quantity": 1,
    "amount": 1500
  }'
```

**Expected:** Transaction fails, order is cancelled (compensated).

---

### Scenario 3: Check Transaction Status
```bash
# Get all transactions
curl http://localhost:5000/api/saga

# Get specific transaction
curl http://localhost:5000/api/saga/{transactionId}
```

---

### Scenario 4: Direct Service Calls

**Create Order Directly:**
```bash
curl -X POST http://localhost:5001/api/orders \
  -H "Content-Type: application/json" \
  -d '{
    "customerId": "CUST001",
    "productId": "PROD001",
    "quantity": 1,
    "amount": 1000
  }'
```

**Process Payment Directly:**
```bash
curl -X POST http://localhost:5002/api/payments \
  -H "Content-Type: application/json" \
  -d '{
    "orderId": "{orderId}",
    "customerId": "CUST001",
    "amount": 1000
  }'
```

---

## 🔄 Saga Flow

### Happy Path
```
Client → Saga API Gateway
         ├─→ Order Service: Create Order
         ├─→ Payment Service: Process Payment
         └─→ Order Service: Confirm Order
         
Result: Transaction Completed
```

### Failure Path with Compensation
```
Client → Saga API Gateway
         ├─→ Order Service: Create Order ✓
         ├─→ Payment Service: Process Payment ✗
         │
         └─→ COMPENSATION:
             ├─→ Order Service: Cancel Order
             
Result: Transaction Compensated
```

---

## 🛠️ PowerShell Testing

### Test with PowerShell
```powershell
# Successful transaction
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

### Check Customer Balance
```powershell
Invoke-RestMethod -Uri "http://localhost:5002/api/payments/customers/CUST001/balance"
```

### Get All Orders
```powershell
Invoke-RestMethod -Uri "http://localhost:5001/api/orders"
```

---

## 📝 Response Status Codes

### Success Codes
- `200 OK` - Request successful
- `201 Created` - Resource created

### Client Error Codes
- `400 Bad Request` - Invalid request or business rule violation
- `404 Not Found` - Resource not found

### Server Error Codes
- `500 Internal Server Error` - Unexpected server error

---

## 🔍 Troubleshooting

### Services Won't Start
```powershell
# Check if ports are in use
netstat -ano | findstr "5000 5001 5002"

# Kill processes using the ports
Stop-Process -Id <PID> -Force
```

### Connection Refused
- Ensure all services are running
- Check firewall settings
- Verify ports are not blocked

### Swagger Not Loading
- Navigate to: http://localhost:{port}/swagger
- Ensure service is running
- Check console output for errors

---

## 🎯 Key Features

✅ **Distributed Transactions** - Coordinated across multiple services  
✅ **Automatic Compensation** - Rollback on failures  
✅ **RESTful APIs** - Standard HTTP endpoints  
✅ **Swagger Documentation** - Interactive API docs  
✅ **Async Communication** - HttpClient-based  
✅ **Comprehensive Logging** - Detailed execution traces  
✅ **Thread-Safe** - Concurrent request handling  

---

## 📚 Additional Resources

- Swagger UI: http://localhost:5000/swagger (Saga Gateway)
- Swagger UI: http://localhost:5001/swagger (Order Service)
- Swagger UI: http://localhost:5002/swagger (Payment Service)

---

**For more details, see:**
- [README.md](README.md) - Project overview
- [IMPLEMENTATION_GUIDE.md](IMPLEMENTATION_GUIDE.md) - Pattern details
- [ARCHITECTURE.md](ARCHITECTURE.md) - System architecture
