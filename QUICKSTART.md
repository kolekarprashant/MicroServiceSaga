# Quick Start Guide

## 🚀 Getting Started in 2 Minutes

### Step 1: Build the Solution
```powershell
dotnet build MassTransitSagaDemo.sln
```

### Step 2: Run All Services
```powershell
.\run-all-services.ps1
```

This opens 4 terminal windows:
- 🎯 **Saga Orchestrator** (coordinates workflows)
- 🛒 **Order Service** (submits orders)
- 💳 **Payment Service** (processes payments)
- 📦 **Inventory Service** (manages inventory)

### Step 3: Watch the Magic! ✨

You'll see output like:

**Order Service:**
```
🛒 OrderService: Submitting order abc123 for $245.50 (PROD-1234 x 5)
✅ OrderService: Order abc123 completed successfully! Amount: $245.50
```

**Saga Orchestrator:**
```
🎯 Saga Orchestrator: Order abc123 submitted - Amount: $245.50
🎯 Saga Orchestrator: Inventory reserved for abc123, initiating payment...
🎯 Saga Orchestrator: Payment processed for abc123, order completed!
```

**Inventory Service:**
```
📦 InventoryService: Reserving inventory for Order abc123 - Product: PROD-1234, Qty: 5
📦 InventoryService: Inventory reserved successfully for Order abc123
```

**Payment Service:**
```
💳 PaymentService: Processing payment for Order abc123, Amount: $245.50
💳 PaymentService: Payment SUCCESSFUL for Order abc123
```

---

### What Happens When Payment Fails?

**Payment Service:**
```
💳 PaymentService: Payment FAILED for Order def456. Reason: Insufficient funds
```

**Saga Orchestrator (Compensation):**
```
🎯 Saga Orchestrator: Payment failed for def456 - Insufficient funds
🎯 Saga Orchestrator: Initiating compensation - releasing inventory...
🎯 Saga Orchestrator: Inventory released (compensation) for def456
```

**Inventory Service (Compensation):**
```
📦 InventoryService: Releasing inventory for Order def456 (Compensation)
📦 InventoryService: Inventory released for Order def456
```

**Order Service:**
```
❌ OrderService: Order def456 failed! Reason: Payment failed - order cancelled
```

---

## 🎯 Testing Scenarios

### Successful Order Flow
1. Order submitted
2. Inventory reserved
3. Payment processed ✅
4. Order completed

### Failed Order with Compensation
1. Order submitted
2. Inventory reserved
3. Payment failed ❌
4. **Inventory released (compensation)**
5. Order failed

---

## 🔧 Manual Service Start (Alternative)

If you prefer to start services manually:

**Terminal 1:**
```powershell
dotnet run --project src\MassTransitSagaDemo\MassTransitSagaDemo.csproj
```

**Terminal 2:**
```powershell
dotnet run --project src\OrderService\OrderService.csproj
```

**Terminal 3:**
```powershell
dotnet run --project src\PaymentService\PaymentService.csproj
```

**Terminal 4:**
```powershell
dotnet run --project src\InventoryService\InventoryService.csproj
```

---

## 📊 Understanding the Output

| Icon | Meaning |
|------|---------|
| 🎯   | Saga Orchestrator activity |
| 🛒   | Order Service activity |
| 💳   | Payment Service activity |
| 📦   | Inventory Service activity |
| ✅   | Successful operation |
| ❌   | Failed operation |

---

## 🛑 Stopping Services

Press **Ctrl+C** in each terminal window to stop the services gracefully.

---

## 💡 Tips

- Orders are submitted automatically every 10 seconds
- Payment failures are simulated randomly (~20-40% failure rate)
- Higher amounts (>$300) have higher failure rates
- Watch all 4 windows to see the complete flow
- The saga automatically handles compensation on failure

---

## 🐛 Troubleshooting

**Services not finding each other?**
- Ensure all 4 services are running
- They use in-memory transport (must be running simultaneously)

**No orders being processed?**
- Wait up to 10 seconds for first order
- Check Order Service is running

**Build errors?**
- Run `dotnet restore` first
- Ensure .NET 8 SDK is installed

---

## 📚 Next Steps

1. Review `README.md` for detailed documentation
2. Check `ARCHITECTURE.md` for system design
3. Explore the code in each service
4. Modify payment failure logic in `PaymentService/ProcessPaymentConsumer.cs`
5. Add your own services to the saga!

---

## 🎓 Learning Resources

- **Saga Pattern**: https://microservices.io/patterns/data/saga.html
- **MassTransit**: https://masstransit.io/
- **State Machines**: https://masstransit.io/documentation/patterns/saga/state-machine

---

**Enjoy exploring the Saga Orchestration pattern!** 🎉
