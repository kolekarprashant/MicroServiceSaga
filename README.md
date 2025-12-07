# Saga Orchestration Pattern - C# Implementation

A complete implementation of the **Saga Orchestration Pattern** for managing distributed transactions across microservices with automatic compensation for handling failures.

## 🏗️ Architecture

The system demonstrates a distributed transaction workflow with three main services:

- **Order Service**: Manages order lifecycle (create, confirm, cancel)
- **Payment Service**: Processes payments and refunds with balance validation
- **Inventory Service**: Handles stock reservations and releases
- **Saga Orchestrator**: Coordinates the entire transaction flow and manages compensations

## ✨ Features

- ✅ **Distributed Transaction Management**: Coordinates multiple services in a single transaction
- ✅ **Automatic Compensation**: Rollback mechanism when any step fails
- ✅ **Failed Transaction Handling**: Demonstrates various failure scenarios
- ✅ **Step-by-Step Execution**: Clear visibility into each saga step
- ✅ **Idempotent Operations**: Safe retry mechanisms
- ✅ **Comprehensive Logging**: Detailed console output for debugging

## 🚀 Getting Started

### Prerequisites

- .NET 8.0 SDK or later

### Build and Run

```bash
# Navigate to the project directory
cd c:\personal\sagaPattern

# Restore dependencies and build
dotnet build

# Run the application
dotnet run --project SagaPattern\SagaPattern.csproj
```

## 📋 Demo Scenarios

The application demonstrates four scenarios:

### 1. **Successful Transaction** ✓
- Customer with sufficient balance
- Product in stock
- All steps complete successfully

### 2. **Payment Failure** ✗
- Customer with insufficient funds
- Inventory is reserved but payment fails
- **Compensation**: Inventory is released, order is cancelled

### 3. **Inventory Failure** ✗
- Product out of stock
- Order creation succeeds but inventory reservation fails
- **Compensation**: Order is cancelled

### 4. **Simulated System Failure** ✗
- Simulates payment system unavailability
- Demonstrates compensation of multiple steps
- **Compensation**: Inventory released, order cancelled

## 🔄 Saga Flow

### Happy Path
```
1. Create Order
2. Reserve Inventory
3. Process Payment
4. Confirm Order → SUCCESS
```

### Failure Path with Compensation
```
1. Create Order ✓
2. Reserve Inventory ✓
3. Process Payment ✗
4. Compensation:
   - Refund Payment (if needed)
   - Release Inventory
   - Cancel Order
```

## 🏛️ Project Structure

```
SagaPattern/
├── SagaPattern.sln
└── SagaPattern/
    ├── SagaPattern.csproj
    ├── Program.cs                  # Main entry point with demo scenarios
    ├── Models/
    │   ├── Enums.cs               # Status enumerations
    │   ├── Order.cs               # Order domain model
    │   ├── Payment.cs             # Payment domain model
    │   ├── Inventory.cs           # Inventory domain models
    │   └── SagaTransaction.cs     # Saga state tracking
    └── Services/
        ├── OrderService.cs         # Order management service
        ├── PaymentService.cs       # Payment processing service
        ├── InventoryService.cs     # Inventory management service
        └── SagaOrchestrator.cs     # Main orchestration logic
```

## 🎯 Key Concepts Demonstrated

1. **Saga Pattern**: Manages distributed transactions as a sequence of local transactions
2. **Compensation Logic**: Reverses completed steps when a failure occurs
3. **State Management**: Tracks saga execution state and steps
4. **Idempotency**: Ensures operations can be safely retried
5. **Fault Tolerance**: Graceful handling of service failures

## 💡 Code Highlights

### Saga Orchestrator
The orchestrator executes steps sequentially and triggers compensation on failure:

```csharp
public async Task<SagaTransaction> ExecuteSaga(...)
{
    try {
        // Execute steps
        CreateOrder();
        ReserveInventory();
        ProcessPayment();
        ConfirmOrder();
    }
    catch {
        // Compensate in reverse order
        CompensateSaga();
    }
}
```

### Compensation Logic
Executed in reverse order of the original steps:

```csharp
- Refund Payment
- Release Inventory  
- Cancel Order
```

## 📊 Console Output

The application provides detailed, color-coded console output showing:
- Initial system state (balances, inventory)
- Each saga step execution
- Success/failure indicators
- Compensation steps (when failures occur)
- Final system state summary

## 🔧 Customization

You can modify the scenarios by adjusting:
- Customer balances in `PaymentService` constructor
- Initial inventory in `InventoryService` constructor
- Failure simulation flags in scenario methods

## 📚 Learning Resources

This implementation demonstrates:
- Microservices transaction patterns
- Distributed systems failure handling
- Event-driven architecture concepts
- ACID vs BASE transactions

## 🤝 Contributing

Feel free to extend this implementation with:
- Persistence layer (database)
- Message queue integration (RabbitMQ, Kafka)
- Event sourcing
- Additional services (shipping, notification)

## 📝 License

MIT License - Free to use for learning and commercial purposes
