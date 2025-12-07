# SAGA PATTERN - ARCHITECTURE DIAGRAM

## System Architecture

```
┌─────────────────────────────────────────────────────────────────────────┐
│                            CLIENT APPLICATION                            │
│                              (Program.cs)                                │
└────────────────────────────────┬────────────────────────────────────────┘
                                 │
                                 ▼
┌─────────────────────────────────────────────────────────────────────────┐
│                         SAGA ORCHESTRATOR                                │
│                     (SagaOrchestrator.cs)                               │
│                                                                          │
│  - Coordinates transaction flow                                         │
│  - Manages saga state                                                    │
│  - Handles compensation logic                                           │
│  - Tracks executed and compensated steps                                │
└────────┬─────────────────────┬─────────────────────┬────────────────────┘
         │                     │                     │
         ▼                     ▼                     ▼
┌────────────────┐    ┌────────────────┐    ┌────────────────────┐
│ ORDER SERVICE  │    │ INVENTORY SVC  │    │  PAYMENT SERVICE   │
│                │    │                │    │                    │
│ - CreateOrder  │    │ - ReserveStock │    │ - ProcessPayment   │
│ - ConfirmOrder │    │ - ReleaseStock │    │ - RefundPayment    │
│ - CancelOrder  │    │ - ConfirmResvn │    │ - GetBalance       │
└────────┬───────┘    └────────┬───────┘    └─────────┬──────────┘
         │                     │                       │
         ▼                     ▼                       ▼
┌────────────────┐    ┌────────────────┐    ┌────────────────────┐
│  Order Store   │    │ Inventory Store│    │   Payment Store    │
│ (In-Memory)    │    │  (In-Memory)   │    │   (In-Memory)      │
└────────────────┘    └────────────────┘    └────────────────────┘
```

## Transaction Flow

### SUCCESSFUL TRANSACTION PATH

```
START
  │
  ├─► [1] Create Order
  │    ├─ OrderService.CreateOrder()
  │    └─ State: OrderCreated ✓
  │
  ├─► [2] Reserve Inventory
  │    ├─ InventoryService.ReserveStock()
  │    └─ State: InventoryReserved ✓
  │
  ├─► [3] Process Payment
  │    ├─ PaymentService.ProcessPayment()
  │    └─ State: PaymentProcessed ✓
  │
  ├─► [4] Confirm Transaction
  │    ├─ OrderService.ConfirmOrder()
  │    ├─ InventoryService.ConfirmReservation()
  │    └─ State: Completed ✓
  │
SUCCESS
```

### FAILED TRANSACTION PATH (With Compensation)

```
START
  │
  ├─► [1] Create Order
  │    ├─ OrderService.CreateOrder()
  │    └─ State: OrderCreated ✓
  │
  ├─► [2] Reserve Inventory
  │    ├─ InventoryService.ReserveStock()
  │    └─ State: InventoryReserved ✓
  │
  ├─► [3] Process Payment
  │    ├─ PaymentService.ProcessPayment()
  │    └─ State: Failed ✗
  │
  ├─► COMPENSATION (Reverse Order)
  │    │
  │    ├─► [3] Compensate Payment (N/A - didn't succeed)
  │    │
  │    ├─► [2] Compensate Inventory
  │    │    ├─ InventoryService.ReleaseReservation()
  │    │    └─ State: InventoryReleased ✓
  │    │
  │    └─► [1] Compensate Order
  │         ├─ OrderService.CancelOrder()
  │         └─ State: OrderCancelled ✓
  │
COMPENSATED (System returned to consistent state)
```

## Data Models Relationship

```
┌──────────────────┐
│  SagaTransaction │
│                  │
│  - TransactionId │◄────────────┐
│  - OrderId       │─────┐       │
│  - PaymentId     │──┐  │       │
│  - ReservationId │─┐│  │       │
│  - State         │ ││  │       │
│  - ExecutedSteps │ ││  │       │
└──────────────────┘ ││  │       │
                     ││  │       │
         ┌───────────┘│  │       │
         │   ┌────────┘  │       │
         │   │  ┌────────┘       │
         │   │  │                │
         ▼   ▼  ▼                │
    ┌─────────────┐              │
    │   Payment   │              │
    │             │              │
    │ - PaymentId │              │
    │ - OrderId   │──────┐       │
    │ - Amount    │      │       │
    │ - Status    │      │       │
    └─────────────┘      │       │
                         ▼       │
                    ┌─────────┐  │
                    │  Order  │  │
                    │         │  │
                    │ - OrderId│ │
                    │ - ProductId│─┐
                    │ - Status │  │
                    └─────────┘  │
                                 │
                                 ▼
                    ┌──────────────────────┐
                    │ InventoryReservation │
                    │                      │
                    │ - ReservationId      │
                    │ - OrderId            │
                    │ - ProductId          │
                    │ - Quantity           │
                    │ - Status             │
                    └──────────────────────┘
```

## State Transitions

### Saga State Machine

```
                    ┌─────────┐
                    │ Started │
                    └────┬────┘
                         │
                         ▼
                  ┌──────────────┐
             ┌────│ OrderCreated │
             │    └──────┬───────┘
             │           │
             │           ▼
             │    ┌──────────────────┐
             ├────│ InventoryReserved│
             │    └──────┬───────────┘
             │           │
             │           ▼
             │    ┌──────────────────┐
             ├────│ PaymentProcessed │
             │    └──────┬───────────┘
             │           │
             │           ▼
             │    ┌───────────┐
             │    │ Completed │
             │    └───────────┘
             │
             │ (If any step fails)
             │
             ▼
      ┌────────────┐
      │   Failed   │
      └─────┬──────┘
            │
            ▼
      ┌──────────────┐
      │ Compensating │
      └─────┬────────┘
            │
            ▼
      ┌──────────────┐
      │ Compensated  │
      └──────────────┘
```

### Order Status Transitions

```
┌─────────┐
│ Pending │
└────┬────┘
     │
     ├─► Success Path ──────► ┌───────────┐
     │                        │ Confirmed │
     │                        └───────────┘
     │
     └─► Failure Path ──────► ┌───────────┐
                              │ Cancelled │
                              └───────────┘
                                    or
                              ┌───────────┐
                              │  Failed   │
                              └───────────┘
```

### Payment Status Transitions

```
┌─────────┐
│ Pending │
└────┬────┘
     │
     ├─► Success ──────► ┌─────────┐
     │                   │ Success │──► Refund ──► ┌──────────┐
     │                   └─────────┘               │ Refunded │
     │                                             └──────────┘
     └─► Failure ──────► ┌────────┐
                         │ Failed │
                         └────────┘
```

### Inventory Status Transitions

```
┌───────────┐
│ Available │
└─────┬─────┘
      │
      ▼
┌──────────┐    Confirm    ┌──────┐
│ Reserved │──────────────►│ Sold │
└────┬─────┘                └──────┘
     │
     │ Release
     ▼
┌───────────┐
│ Released  │
└───────────┘
```

## Error Handling Strategy

```
┌────────────────────────────────────────────────────────────┐
│                    Try Execute Step                        │
│                                                             │
│  ┌──────────────────────────────────────────────────┐     │
│  │  Step Succeeds?                                   │     │
│  │                                                    │     │
│  │  YES ──► Add to ExecutedSteps ──► Continue       │     │
│  │   │                                               │     │
│  │  NO                                               │     │
│  │   │                                               │     │
│  │   └──► Set ErrorMessage ──► Start Compensation   │     │
│  │                                                    │     │
│  └────────────────────────────────────────────────────┘   │
│                                                             │
│                         │                                  │
│                         ▼                                  │
│            ┌──────────────────────────┐                   │
│            │  Compensation Process     │                   │
│            │                          │                   │
│            │  For each ExecutedStep   │                   │
│            │    (in reverse order)    │                   │
│            │                          │                   │
│            │  Execute Compensation    │                   │
│            │  Add to CompensatedSteps │                   │
│            └──────────────────────────┘                   │
└────────────────────────────────────────────────────────────┘
```

## Thread Safety

All services implement thread-safe operations using locks:

```
┌─────────────────────────────────────────┐
│          Service Method Call            │
└──────────────────┬──────────────────────┘
                   │
                   ▼
        ┌──────────────────────┐
        │   Acquire Lock       │
        │   lock(_lock)        │
        └──────────┬───────────┘
                   │
                   ▼
        ┌──────────────────────┐
        │  Critical Section    │
        │  - Read Data         │
        │  - Modify Data       │
        │  - Write Data        │
        └──────────┬───────────┘
                   │
                   ▼
        ┌──────────────────────┐
        │   Release Lock       │
        │   (automatic)        │
        └──────────────────────┘
```

## Performance Flow

```
Time ──────────────────────────────────────────────────►

Step 1: Create Order          [====]
Step 2: Reserve Inventory           [====]
Step 3: Process Payment (500ms)           [========]
Step 4: Confirm                                   [====]

Total Time: ~1-2 seconds (including delays)
```

With Compensation (on failure):

```
Time ──────────────────────────────────────────────────►

Step 1: Create Order          [====]
Step 2: Reserve Inventory           [====]
Step 3: Process Payment (FAIL)            [====] ✗

Compensation Phase:
  Refund Payment (if needed)                  [==]
  Release Inventory (200ms)                      [====]
  Cancel Order (200ms)                              [====]

Total Time: ~1-2 seconds (including delays)
```

## Deployment Architecture

```
┌──────────────────────────────────────────────────────────┐
│                    Production Deployment                  │
└──────────────────────────────────────────────────────────┘

┌─────────────┐     ┌─────────────┐     ┌─────────────┐
│   Order     │     │  Inventory  │     │   Payment   │
│  Service    │     │   Service   │     │   Service   │
│             │     │             │     │             │
│  + DB       │     │  + DB       │     │  + DB       │
│  + Queue    │     │  + Queue    │     │  + Queue    │
└──────┬──────┘     └──────┬──────┘     └──────┬──────┘
       │                   │                    │
       └───────────────────┼────────────────────┘
                          │
                          ▼
                ┌─────────────────┐
                │     Message     │
                │      Bus        │
                │  (RabbitMQ /    │
                │   Kafka)        │
                └─────────────────┘
                          │
                          ▼
                ┌─────────────────┐
                │      Saga       │
                │  Orchestrator   │
                │                 │
                │  + State Store  │
                │  + Event Log    │
                └─────────────────┘
```

---

This architecture ensures:
✓ Loose coupling between services
✓ Independent scalability
✓ Fault tolerance
✓ Data consistency through compensation
✓ Clear separation of concerns
