# Architecture Overview

## Service Communication Flow

```
┌─────────────────┐
│  Order Service  │ Initiates orders every 10s
└────────┬────────┘
         │ SubmitOrder
         ▼
┌─────────────────────────────────────────────────┐
│         Saga Orchestrator (State Machine)       │
│  States: Submitted → InventoryReserved →        │
│          Completed / Failed                     │
└──────┬──────────────────────────────────┬───────┘
       │                                  │
       │ ReserveInventory                │ ProcessPayment
       ▼                                  ▼
┌──────────────────┐              ┌──────────────────┐
│ Inventory Service│              │  Payment Service │
│                  │              │                  │
│ • Reserve        │              │ • Process        │
│ • Release (comp) │              │ • Simulate fail  │
└──────┬───────────┘              └──────┬───────────┘
       │                                  │
       │ InventoryReserved               │ PaymentProcessed
       │ InventoryReleased               │ PaymentFailed
       │                                  │
       └──────────────┬───────────────────┘
                      ▼
              ┌───────────────┐
              │     Events     │
              │                │
              │ OrderCompleted │
              │ OrderFailed    │
              └───────┬────────┘
                      │
                      ▼
              ┌──────────────────┐
              │  Order Service   │ (receives final status)
              └──────────────────┘
```

## Message Contracts

All services share the same message contracts from `MassTransit.Messages`:

### Commands (Sent to specific services)
- `SubmitOrder` → Triggers the saga
- `ReserveInventory` → Sent to Inventory Service
- `ProcessPayment` → Sent to Payment Service
- `ReleaseInventory` → Compensation command to Inventory Service

### Events (Published to all interested services)
- `InventoryReserved` → Published by Inventory Service
- `PaymentProcessed` → Published by Payment Service (success)
- `PaymentFailed` → Published by Payment Service (failure)
- `InventoryReleased` → Published by Inventory Service (compensation)
- `OrderCompleted` → Published by Saga (final success)
- `OrderFailed` → Published by Saga (final failure)

## State Machine (Saga Orchestrator)

```
┌─────────┐
│ Initial │
└────┬────┘
     │ SubmitOrder
     ▼
┌──────────┐
│Submitted │
└────┬─────┘
     │ InventoryReserved
     ▼
┌───────────────────┐
│InventoryReserved  │
└────┬──────────────┘
     │
     ├─ PaymentProcessed ──────┐
     │                         ▼
     │                    ┌──────────┐
     │                    │Completed │
     │                    └──────────┘
     │
     └─ PaymentFailed ──────────┐
                                ▼
                           ┌────────┐
                           │ Failed │ (+ Compensation)
                           └────────┘
```

## Compensation Logic

When payment fails, the saga automatically executes compensation:

1. **PaymentFailed** event received
2. Saga transitions to **Failed** state
3. Saga publishes **ReleaseInventory** (compensation)
4. Inventory Service releases the reserved inventory
5. Inventory Service publishes **InventoryReleased**
6. Saga publishes **OrderFailed** to Order Service

## Running the Demo

1. Start all services (use `run-all-services.ps1`)
2. Watch the Order Service submit orders every 10 seconds
3. Observe successful flows (green checkmarks ✅)
4. Observe failed flows with compensation (red X ❌)

## Key Patterns Demonstrated

1. **Saga Orchestration** - Central coordinator pattern
2. **Compensation** - Automatic rollback on failure
3. **Event-Driven Architecture** - Services communicate via events
4. **Service Independence** - Each service is autonomous
5. **State Management** - Saga maintains workflow state

## Technology Stack

- **MassTransit 8.2.3** - Message bus abstraction
- **.NET 8** - Runtime
- **In-Memory Transport** - For demo (replace with RabbitMQ/Azure Service Bus in production)
- **In-Memory Saga Repository** - For demo (replace with SQL/MongoDB in production)
