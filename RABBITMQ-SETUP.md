# RabbitMQ Setup Guide

## ✅ All Services Converted to RabbitMQ

All four services have been successfully converted from in-memory transport to RabbitMQ:

### Changes Made:
1. ✅ Added `MassTransit.RabbitMQ` package (v8.2.3) to all services
2. ✅ Updated all `Program.cs` files to use `UsingRabbitMq` instead of `UsingInMemory`
3. ✅ Configured RabbitMQ connection: `localhost`, virtual host `/`, credentials `guest/guest`

### Services Updated:
- ✅ Saga Orchestrator (Port 5000)
- ✅ Order Service (Port 5001)
- ✅ Payment Service (Port 5002)
- ✅ Inventory Service (Port 5003)

---

## 🐰 Install RabbitMQ

### Option 1: Docker (Recommended - Easiest)

Run this command to start RabbitMQ with management UI:

```powershell
docker run -d --name rabbitmq -p 5672:5672 -p 15672:15672 rabbitmq:3-management
```

**Management UI:** http://localhost:15672
- Username: `guest`
- Password: `guest`

### Option 2: Windows Installer

1. **Install Erlang** (RabbitMQ requires it):
   - Download from: https://www.erlang.org/downloads
   - Install with default settings

2. **Install RabbitMQ**:
   - Download from: https://www.rabbitmq.com/download.html
   - Install with default settings

3. **Enable Management Plugin**:
   ```powershell
   cd "C:\Program Files\RabbitMQ Server\rabbitmq_server-3.x.x\sbin"
   .\rabbitmq-plugins.bat enable rabbitmq_management
   ```

4. **Start RabbitMQ Service**:
   - Open Services (Win + R → `services.msc`)
   - Find "RabbitMQ" service
   - Start it

---

## 🚀 Start All Services

### Step 1: Restore Packages (if not done already)
```powershell
dotnet restore
```

### Step 2: Start Services

**Option A - Use PowerShell Script:**
```powershell
.\run-all-services.ps1
```

**Option B - Manually in Separate Terminals:**

Terminal 1:
```powershell
cd src\MassTransitSagaDemo
dotnet run
```

Terminal 2:
```powershell
cd src\OrderService
dotnet run
```

Terminal 3:
```powershell
cd src\PaymentService
dotnet run
```

Terminal 4:
```powershell
cd src\InventoryService
dotnet run
```

---

## 🧪 Test the System

### 1. Verify RabbitMQ is Running

Open RabbitMQ Management UI: http://localhost:15672

Check that queues are created automatically when services start:
- `OrderService-OrderCompletedConsumer`
- `OrderService-OrderFailedConsumer`
- `PaymentService-ProcessPaymentConsumer`
- `InventoryService-ReserveInventoryConsumer`
- `InventoryService-ReleaseInventoryConsumer`
- `order-state` (saga state machine)

### 2. Submit an Order

**Using PowerShell:**
```powershell
$body = @{
    orderId = "ORDER-001"
    customerId = "CUST-123"
    amount = 100.50
} | ConvertTo-Json

Invoke-RestMethod -Uri "http://localhost:5001/api/orders" -Method Post -Body $body -ContentType "application/json"
```

**Using curl:**
```bash
curl -X POST http://localhost:5001/api/orders -H "Content-Type: application/json" -d "{\"orderId\":\"ORDER-001\",\"customerId\":\"CUST-123\",\"amount\":100.50}"
```

### 3. Check Order Status

```powershell
Invoke-RestMethod -Uri "http://localhost:5001/api/orders/ORDER-001"
```

### 4. Check Inventory Records

**This should NOW work with RabbitMQ!**

```powershell
Invoke-RestMethod -Uri "http://localhost:5003/api/inventory/order/ORDER-001"
```

### 5. Monitor RabbitMQ Messages

Go to RabbitMQ Management UI → Queues → Click on any queue → "Get Messages" to see messages being processed.

---

## 📊 Expected Workflow

1. **Submit Order** → Order Service publishes `SubmitOrder` event
2. **Saga receives event** → Transitions to "Submitted" state
3. **Saga sends** `ReserveInventory` command → Inventory Service
4. **Inventory reserved** → Saga receives `InventoryReserved` event
5. **Saga sends** `ProcessPayment` command → Payment Service
6. **Payment processed** → Two outcomes:
   - ✅ **Success**: Saga completes → Order marked complete
   - ❌ **Failure**: Saga compensates → Sends `ReleaseInventory` → Order marked failed

---

## 🔍 Troubleshooting

### Services Won't Start - "Address already in use"
Some processes are still running on the ports. Kill them:
```powershell
Get-Process | Where-Object {$_.ProcessName -like "*dotnet*"} | Stop-Process -Force
```

### RabbitMQ Connection Errors
Check RabbitMQ is running:
```powershell
# Docker
docker ps | Select-String rabbitmq

# Windows Service
Get-Service rabbitmq
```

### No Queues Visible in RabbitMQ
1. Make sure all 4 services are running
2. Wait 10 seconds after startup
3. Refresh RabbitMQ Management UI

### Inventory Records Still Not Showing
1. Verify RabbitMQ Management UI shows message exchanges
2. Check all service logs for errors
3. Verify `InventoryService` console shows "📥 Received ReserveInventory" message

---

## 🎯 Key Differences: In-Memory vs RabbitMQ

| Feature | In-Memory | RabbitMQ |
|---------|-----------|----------|
| Cross-Process | ❌ No | ✅ Yes |
| Service Communication | ❌ Isolated | ✅ Connected |
| Message Persistence | ❌ No | ✅ Yes (optional) |
| Monitoring | ❌ None | ✅ Management UI |
| Production Ready | ❌ No | ✅ Yes |

**The in-memory bus problem is now SOLVED!** Services can communicate across processes using RabbitMQ.
