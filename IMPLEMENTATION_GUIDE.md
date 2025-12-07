# SAGA PATTERN IMPLEMENTATION GUIDE

## Overview

This document explains the Saga Orchestration Pattern implementation for handling distributed transactions with compensation logic.

## What is the Saga Pattern?

The Saga pattern is a design pattern for managing data consistency across microservices in distributed transaction scenarios. Unlike traditional ACID transactions, which work within a single database, sagas handle transactions across multiple services by breaking them into a sequence of local transactions.

### Key Concepts

1. **Local Transactions**: Each service manages its own database and exposes operations
2. **Compensating Transactions**: Reverse operations to undo completed steps
3. **Orchestration**: A central coordinator manages the saga workflow

## Architecture Components

### 1. Domain Models

#### Order (`Models/Order.cs`)
Represents a customer order with status tracking.

```csharp
- OrderId: Unique identifier
- CustomerId: Customer reference
- ProductId: Product reference
- Quantity: Number of items
- Amount: Total price
- Status: Pending, Confirmed, Cancelled, Failed
```

#### Payment (`Models/Payment.cs`)
Tracks payment transactions and their outcomes.

```csharp
- PaymentId: Unique identifier
- OrderId: Associated order
- Amount: Payment amount
- Status: Pending, Success, Failed, Refunded
- FailureReason: Error details if failed
```

#### InventoryReservation (`Models/Inventory.cs`)
Manages stock reservations for orders.

```csharp
- ReservationId: Unique identifier
- OrderId: Associated order
- ProductId: Product reference
- Quantity: Reserved amount
- Status: Reserved, Released
```

#### SagaTransaction (`Models/SagaTransaction.cs`)
Tracks the saga execution state.

```csharp
- TransactionId: Unique identifier
- State: Started, Completed, Failed, Compensating, Compensated
- ExecutedSteps: List of completed steps
- CompensatedSteps: List of reversed steps
- ErrorMessage: Failure details
```

### 2. Services

#### OrderService
Manages order lifecycle operations.

**Operations:**
- `CreateOrder()`: Creates a new pending order
- `ConfirmOrder()`: Marks order as confirmed
- `CancelOrder()`: Cancels order with reason
- `MarkOrderFailed()`: Marks order as failed

**Compensation:**
- Cancel the order if saga fails

#### PaymentService
Handles payment processing and refunds.

**Operations:**
- `ProcessPayment()`: Attempts to charge customer
  - Validates customer exists
  - Checks sufficient balance
  - Deducts amount from balance
- `RefundPayment()`: Returns money to customer

**Compensation:**
- Refund the payment if subsequent steps fail

#### InventoryService
Manages stock reservations and releases.

**Operations:**
- `ReserveStock()`: Reserves inventory for order
  - Validates product exists
  - Checks stock availability
  - Moves stock from available to reserved
- `ReleaseReservation()`: Returns reserved stock
- `ConfirmReservation()`: Finalizes reservation

**Compensation:**
- Release reserved inventory if order fails

### 3. Saga Orchestrator

The `SagaOrchestrator` is the heart of the pattern. It coordinates the entire transaction flow.

#### Execution Flow

```
ExecuteSaga()
    ├─ Step 1: Create Order
    │   └─ OrderService.CreateOrder()
    │
    ├─ Step 2: Reserve Inventory
    │   └─ InventoryService.ReserveStock()
    │   └─ If fails → Compensate
    │
    ├─ Step 3: Process Payment
    │   └─ PaymentService.ProcessPayment()
    │   └─ If fails → Compensate
    │
    └─ Step 4: Confirm Order
        └─ OrderService.ConfirmOrder()
        └─ InventoryService.ConfirmReservation()
```

#### Compensation Flow

When a step fails, the orchestrator executes compensation in **reverse order**:

```
CompensateSaga()
    ├─ Compensate: Payment (if processed)
    │   └─ PaymentService.RefundPayment()
    │
    ├─ Compensate: Inventory (if reserved)
    │   └─ InventoryService.ReleaseReservation()
    │
    └─ Compensate: Order (if created)
        └─ OrderService.CancelOrder()
```

## Failure Scenarios

### Scenario 1: Successful Transaction ✓

**Setup:**
- Customer: CUST001 (balance: $1000)
- Product: PROD001 (stock: 10 units)
- Order: 2 units @ $2000

**Flow:**
1. Create Order → Success
2. Reserve Inventory (2 units) → Success
3. Process Payment ($2000) → Success
4. Confirm Order → Success

**Result:**
- Order Status: Confirmed
- Customer Balance: $-1000 (overdraft not prevented in demo)
- Inventory: 8 units available, 0 reserved

### Scenario 2: Payment Failure (Insufficient Funds) ✗

**Setup:**
- Customer: CUST002 (balance: $50)
- Product: PROD001 (stock: 8 units)
- Order: 1 unit @ $1500

**Flow:**
1. Create Order → Success
2. Reserve Inventory (1 unit) → Success
3. Process Payment ($1500) → **FAILED** (Insufficient funds)
4. **Compensation starts:**
   - Release Inventory (1 unit)
   - Cancel Order

**Result:**
- Order Status: Cancelled
- Customer Balance: $50 (unchanged)
- Inventory: 9 units available, 0 reserved

### Scenario 3: Inventory Failure (Out of Stock) ✗

**Setup:**
- Customer: CUST003 (balance: $5000)
- Product: PROD002 (stock: 2 units)
- Order: 5 units @ $150

**Flow:**
1. Create Order → Success
2. Reserve Inventory (5 units) → **FAILED** (Out of stock)
3. **Compensation starts:**
   - Cancel Order

**Result:**
- Order Status: Cancelled
- Customer Balance: $5000 (unchanged)
- Inventory: 2 units available, 0 reserved

### Scenario 4: Simulated System Failure ✗

**Setup:**
- Customer: CUST001 (balance: $-1000 from Scenario 1)
- Product: PROD003 (stock: 50 units)
- Order: 3 units @ $300
- simulatePaymentFailure: true

**Flow:**
1. Create Order → Success
2. Reserve Inventory (3 units) → Success
3. Process Payment ($300) → **FAILED** (Simulated failure)
4. **Compensation starts:**
   - Release Inventory (3 units)
   - Cancel Order

**Result:**
- Order Status: Cancelled
- Customer Balance: $-1000 (unchanged)
- Inventory: 50 units available, 0 reserved

## Design Patterns Used

### 1. Orchestration Pattern
- Central coordinator manages the saga
- Services don't know about each other
- Clear separation of concerns

### 2. Command Pattern
- Each service operation is a command
- Commands can be undone (compensation)

### 3. State Pattern
- Saga maintains state throughout execution
- State transitions are well-defined

### 4. Repository Pattern (Simplified)
- In-memory dictionaries simulate data stores
- Thread-safe with locking

## Thread Safety

All services use locks to ensure thread-safe operations:

```csharp
private readonly object _lock = new();

lock (_lock)
{
    // Critical section
}
```

## Best Practices Demonstrated

1. **Idempotency**: Operations can be safely retried
2. **Atomicity**: Each service operation is atomic
3. **Consistency**: Compensation ensures consistent state
4. **Isolation**: Services are independent
5. **Durability**: State is tracked (in-memory for demo)

## Extending the Implementation

### Add Database Persistence
Replace in-memory dictionaries with Entity Framework:

```csharp
public class OrderService
{
    private readonly ApplicationDbContext _context;
    
    public Order CreateOrder(...)
    {
        var order = new Order { ... };
        _context.Orders.Add(order);
        _context.SaveChanges();
        return order;
    }
}
```

### Add Message Queue
Use RabbitMQ or Azure Service Bus for async communication:

```csharp
public async Task ExecuteSaga(...)
{
    await _messageBus.Publish(new OrderCreatedEvent { ... });
    await _messageBus.Publish(new InventoryReservedEvent { ... });
}
```

### Add Event Sourcing
Store all saga events for replay and auditing:

```csharp
public class SagaEventStore
{
    public void AppendEvent(SagaEvent evt) { ... }
    public List<SagaEvent> GetEvents(string sagaId) { ... }
}
```

## Testing Strategies

### Unit Tests
Test individual service operations:

```csharp
[Test]
public void CreateOrder_ShouldReturnOrder()
{
    var service = new OrderService();
    var order = service.CreateOrder("CUST001", "PROD001", 1, 100);
    Assert.AreEqual(OrderStatus.Pending, order.Status);
}
```

### Integration Tests
Test saga orchestration:

```csharp
[Test]
public async Task ExecuteSaga_WithInsufficientFunds_ShouldCompensate()
{
    var saga = await _orchestrator.ExecuteSaga("CUST002", "PROD001", 1, 1500);
    Assert.AreEqual(SagaState.Compensated, saga.State);
}
```

## Performance Considerations

1. **Async Operations**: Use `async/await` for I/O operations
2. **Timeouts**: Implement timeouts for each step
3. **Circuit Breaker**: Prevent cascading failures
4. **Retry Logic**: Retry transient failures

## Monitoring and Observability

Add logging and metrics:

```csharp
public async Task<SagaTransaction> ExecuteSaga(...)
{
    _logger.LogInformation("Saga started: {TransactionId}", saga.TransactionId);
    _metrics.IncrementCounter("saga.started");
    
    try {
        // Execute steps
    }
    catch (Exception ex) {
        _logger.LogError(ex, "Saga failed: {TransactionId}", saga.TransactionId);
        _metrics.IncrementCounter("saga.failed");
    }
}
```

## Conclusion

This implementation demonstrates the core concepts of the Saga Orchestration Pattern:

✓ **Distributed Transaction Management**
✓ **Automatic Compensation on Failure**
✓ **Clear Service Boundaries**
✓ **State Tracking and Observability**
✓ **Multiple Failure Scenarios**

The pattern is essential for building resilient microservices architectures where maintaining data consistency across services is critical.
