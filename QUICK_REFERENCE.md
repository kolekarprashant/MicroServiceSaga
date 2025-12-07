# 🚀 QUICK REFERENCE - SAGA PATTERN PROJECT

## One-Line Run Commands

```powershell
# Quick Run
.\run.ps1

# Or using .NET CLI
dotnet run --project SagaPattern\SagaPattern.csproj

# Build only
dotnet build

# Clean build
dotnet clean; dotnet build
```

---

## 📂 File Map

### Entry Point
- `Program.cs` - Main application with 4 demo scenarios

### Core Services
- `SagaOrchestrator.cs` - Saga coordinator (172 lines)
- `OrderService.cs` - Order management (95 lines)
- `PaymentService.cs` - Payment processing (120 lines)
- `InventoryService.cs` - Inventory management (160 lines)

### Models
- `Enums.cs` - All status enumerations
- `Order.cs` - Order domain model
- `Payment.cs` - Payment domain model
- `Inventory.cs` - Inventory models
- `SagaTransaction.cs` - Saga state tracker

### Documentation
- `README.md` - Quick start guide
- `IMPLEMENTATION_GUIDE.md` - Deep dive (400+ lines)
- `ARCHITECTURE.md` - Visual diagrams (350+ lines)
- `PROJECT_SUMMARY.md` - Complete overview

---

## 🎯 Key Code Snippets

### Run a Saga Transaction
```csharp
var orchestrator = new SagaOrchestrator(
    orderService, 
    paymentService, 
    inventoryService
);

var saga = await orchestrator.ExecuteSaga(
    customerId: "CUST001",
    productId: "PROD001",
    quantity: 2,
    amount: 2000m
);
```

### Check Transaction Result
```csharp
if (saga.State == SagaState.Completed)
{
    Console.WriteLine("✓ Success!");
}
else if (saga.State == SagaState.Compensated)
{
    Console.WriteLine($"✗ Failed: {saga.ErrorMessage}");
    Console.WriteLine($"Compensated: {saga.CompensatedSteps.Count} steps");
}
```

---

## 🔍 Debugging Tips

### View Transaction Details
```csharp
var transaction = orchestrator.GetTransaction(transactionId);
Console.WriteLine($"State: {transaction.State}");
Console.WriteLine($"Steps: {string.Join(", ", transaction.ExecutedSteps)}");
```

### Check Service State
```csharp
// Customer balance
var balance = paymentService.GetCustomerBalance("CUST001");

// Inventory status
var item = inventoryService.GetInventoryItem("PROD001");

// Order status
var order = orderService.GetOrder(orderId);
```

---

## 📊 Default Test Data

### Customers
| ID      | Balance |
|---------|---------|
| CUST001 | $1000   |
| CUST002 | $50     |
| CUST003 | $5000   |

### Products
| ID      | Name     | Stock |
|---------|----------|-------|
| PROD001 | Laptop   | 10    |
| PROD002 | Mouse    | 2     |
| PROD003 | Keyboard | 50    |

---

## 🎬 Scenario Quick Reference

| # | Scenario | Customer | Product | Qty | Amount | Expected |
|---|----------|----------|---------|-----|--------|----------|
| 1 | Success  | CUST001 | PROD001 | 2   | $2000  | ✓ Complete |
| 2 | Low Funds | CUST002 | PROD001 | 1   | $1500  | ✗ Compensate |
| 3 | No Stock | CUST003 | PROD002 | 5   | $150   | ✗ Compensate |
| 4 | Sys Fail | CUST001 | PROD003 | 3   | $300   | ✗ Compensate |

---

## 🔧 Customization Points

### Add New Customer
```csharp
// In PaymentService constructor
_customerBalances["CUST004"] = 10000m;
```

### Add New Product
```csharp
// In InventoryService constructor
_inventory["PROD004"] = new InventoryItem
{
    ProductId = "PROD004",
    ProductName = "Monitor",
    AvailableStock = 20,
    ReservedStock = 0
};
```

### Create Custom Scenario
```csharp
var saga = await orchestrator.ExecuteSaga(
    customerId: "CUST004",
    productId: "PROD004",
    quantity: 1,
    amount: 500m,
    simulatePaymentFailure: false,
    simulateInventoryFailure: false
);
```

---

## 📈 State Enums

### SagaState
- `Started` - Saga initiated
- `OrderCreated` - Order created
- `InventoryReserved` - Stock reserved
- `PaymentProcessed` - Payment successful
- `Completed` - All steps done ✓
- `Failed` - Step failed ✗
- `Compensating` - Reversing steps
- `Compensated` - Rollback complete

### OrderStatus
- `Pending` - Initial state
- `Confirmed` - Order successful
- `Cancelled` - Order cancelled
- `Failed` - Order failed

### PaymentStatus
- `Pending` - Processing
- `Success` - Payment complete
- `Failed` - Payment declined
- `Refunded` - Money returned

---

## 🐛 Common Issues

### Build Errors
```powershell
# Clean and rebuild
dotnet clean
dotnet build
```

### Port Already in Use
Not applicable - console app (no web server)

### Permission Issues
```powershell
# Run PowerShell as Administrator if needed
Set-ExecutionPolicy -Scope CurrentUser -ExecutionPolicy RemoteSigned
```

---

## 📚 Learn More

1. **Start Here**: README.md
2. **Deep Dive**: IMPLEMENTATION_GUIDE.md
3. **Visual Guide**: ARCHITECTURE.md
4. **Overview**: PROJECT_SUMMARY.md

---

## 💻 IDE Support

### Visual Studio
1. Open `SagaPattern.sln`
2. Set `SagaPattern` as startup project
3. Press F5 to run

### VS Code
1. Open folder: `c:\personal\sagaPattern`
2. Install C# extension
3. Press F5 or run: `dotnet run --project SagaPattern\SagaPattern.csproj`

### Rider
1. Open `SagaPattern.sln`
2. Right-click Program.cs → Run

---

## 🎓 Key Takeaways

✅ **Saga Pattern** - Distributed transaction management
✅ **Orchestration** - Central coordinator pattern
✅ **Compensation** - Automatic rollback on failure
✅ **Microservices** - Service independence
✅ **Fault Tolerance** - Graceful failure handling

---

## 🚀 Next Steps

1. ✅ Run the demo: `.\run.ps1`
2. ✅ Understand the flow: Watch console output
3. ✅ Read the guide: IMPLEMENTATION_GUIDE.md
4. ✅ Customize: Add your own scenarios
5. ✅ Extend: Add persistence, messaging, APIs

---

**Quick Start**: `.\run.ps1` (That's it!)

**Full Docs**: See IMPLEMENTATION_GUIDE.md

**Questions**: Review PROJECT_SUMMARY.md
