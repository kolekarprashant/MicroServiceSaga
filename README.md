# MassTransit Saga Orchestration Demo - Web API Edition

This project demonstrates a **microservices architecture** using **ASP.NET Core Web APIs** with **MassTransit Saga Orchestration** pattern for distributed transaction management.

## 🏗️ Architecture

The solution consists of **4 independent Web API services** + 1 shared library:

1. **Saga Orchestrator API** (Port 5000) - Central coordinator managing the order workflow state machine
2. **Order Service API** (Port 5001) - REST API to submit orders and track order status
3. **Payment Service API** (Port 5002) - REST API for payment processing and statistics
4. **Inventory Service API** (Port 5003) - REST API for inventory management and tracking
5. **Messages Library** - Shared message contracts used by all services

### 🔄 Workflow

```
OrderService            SagaOrchestrator          InventoryService        PaymentService
    |                          |                         |                      |
    |--SubmitOrder------------>|                         |                      |
    |                          |--ReserveInventory------>|                      |
    |                          |<--InventoryReserved-----|                      |
    |                          |--ProcessPayment------------------------------>|
    |                          |                         |                      |
    |                          |<--PaymentProcessed----------------------------| (Success)
    |<--OrderCompleted---------|                         |                      |
    |                          |                         |                      |
    |       OR (if payment fails)                        |                      |
    |                          |<--PaymentFailed-------------------------------| (Failure)
    |                          |--ReleaseInventory------>|                      |
    |                          |<--InventoryReleased-----|                      |
    |<--OrderFailed------------|                         |                      |
```

## 🎯 Key Features

- **Saga Orchestration Pattern**: Central orchestrator coordinates distributed transactions
- **Compensation Logic**: Automatic inventory release when payment fails
- **Service Independence**: Each service runs independently and communicates via messages
- **Fault Tolerance**: Built-in retry and error handling capabilities
- **In-Memory Transport**: Uses MassTransit in-memory transport for demo purposes (all services share the same in-memory bus)

## 📦 Projects

### MassTransit.Messages
Shared library containing all message contracts used across services.

### Order Service
- Submits new orders every 10 seconds
- Listens for `OrderCompleted` and `OrderFailed` events
- Logs final order status

### Payment Service
- Consumes `ProcessPayment` commands
- Simulates payment processing with random success/failure
- Higher failure rate for orders over $300
- Publishes `PaymentProcessed` or `PaymentFailed` events

### Inventory Service
- Consumes `ReserveInventory` and `ReleaseInventory` commands
- Simulates inventory management
- Supports compensation (release) when payment fails

### Saga Orchestrator
- Implements state machine with states: Submitted, InventoryReserved, Completed, Failed
- Orchestrates the entire order workflow
- Handles compensation when payment fails
- Uses in-memory saga repository

## 🚀 Running the Application

### Prerequisites
- .NET 8.0 SDK

### Option 1: Run All Web APIs at Once (Recommended)

```powershell
.\run-all-services.ps1
```

This will start 4 Web API services in separate PowerShell windows:
- Saga Orchestrator: http://localhost:5000/swagger
- Order Service: http://localhost:5001/swagger
- Payment Service: http://localhost:5002/swagger
- Inventory Service: http://localhost:5003/swagger

### Option 2: Manual - Open 4 Terminal Windows

**Terminal 1 - Saga Orchestrator (Port 5000):**
```powershell
dotnet run --project src\MassTransitSagaDemo\MassTransitSagaDemo.csproj
```

**Terminal 2 - Order Service (Port 5001):**
```powershell
dotnet run --project src\OrderService\OrderService.csproj
```

**Terminal 3 - Payment Service (Port 5002):**
```powershell
dotnet run --project src\PaymentService\PaymentService.csproj
```

**Terminal 4 - Inventory Service (Port 5003):**
```powershell
dotnet run --project src\InventoryService\InventoryService.csproj
```

### Build All Projects
```powershell
dotnet build MassTransitSagaDemo.sln
```

## 🧪 Testing the APIs

### Quick Test - Submit an Order
```bash
curl -X POST http://localhost:5001/api/orders -H "Content-Type: application/json" -d "{\"amount\":150.00,\"productId\":\"PROD-1234\",\"quantity\":2}"
```

**Using PowerShell:**
```powershell
$body = @{ amount = 150.00; productId = "PROD-1234"; quantity = 2 } | ConvertTo-Json
Invoke-RestMethod -Uri "http://localhost:5001/api/orders" -Method POST -Body $body -ContentType "application/json"
```

### Check Order Status
```bash
curl http://localhost:5001/api/orders/{orderId}
```

### View All Orders
```bash
curl http://localhost:5001/api/orders
```

📖 **See [API-TESTING-GUIDE.md](API-TESTING-GUIDE.md) for complete API documentation and test scenarios.**

## 📝 What to Observe

### Using Swagger UI (Recommended)
1. Open http://localhost:5001/swagger
2. Use the POST /api/orders endpoint to submit orders
3. Copy the returned `orderId`
4. Use GET /api/orders/{orderId} to track order status
5. Check other services at their respective Swagger UIs

### Console Output
When you submit orders via API, you'll see real-time logs in each service window:

1. **Order Service** - Receives API requests and tracks order status
2. **Saga Orchestrator** - Coordinates the workflow and manages state
3. **Inventory Service** - Reserves/releases inventory
4. **Payment Service** - Processes payments (some will fail randomly)
5. **Successful Orders**: Order → Inventory Reserved → Payment Success → Order Completed ✅
6. **Failed Orders**: Order → Inventory Reserved → Payment Failed → Inventory Released (Compensation) → Order Failed ❌

## 🎓 Key Learning Points

1. **Web API Architecture**: RESTful services with Swagger documentation
2. **Saga Orchestration Pattern**: Central coordinator manages distributed transactions
3. **Orchestration vs Choreography**: This uses orchestration (central coordinator)
4. **Compensation**: Automatic rollback when steps fail (inventory release)
5. **Message-Based Communication**: Services communicate via MassTransit (in-memory)
6. **State Management**: Saga maintains workflow state across multiple steps
7. **REST + Messaging**: Combines HTTP APIs with event-driven architecture

## 📚 Dependencies

- **ASP.NET Core 8.0** - Web API framework
- **MassTransit** (8.2.3) - Message broker abstraction with Saga support
- **Swashbuckle.AspNetCore** (6.5.0) - Swagger/OpenAPI documentation

## 💡 Production Considerations

For production use, consider:
- Replacing in-memory transport with RabbitMQ or Azure Service Bus
- Adding persistent saga storage (SQL Server, MongoDB)
- Implementing idempotency for all consumers
- Adding health checks and monitoring
- Implementing proper timeout handling
- Adding circuit breakers for external dependencies
- Implementing distributed tracing (OpenTelemetry)

## 🐛 Troubleshooting

**Services not communicating?**
- Ensure all 4 services are running
- Check they're using the same transport (in-memory)
- All services must reference MassTransit.Messages

**Orders not completing?**
- Check all 4 services are running
- Review console logs for errors

## 📖 References

- [MassTransit Documentation](https://masstransit.io/)
- [Saga Pattern](https://microservices.io/patterns/data/saga.html)
- [State Machine Documentation](https://masstransit.io/documentation/patterns/saga/state-machine)

