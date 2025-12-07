using SagaPattern.Models;
using SagaPattern.Services;

namespace SagaPattern
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine(@"
╔══════════════════════════════════════════════════════════════════════════════╗
║                    SAGA ORCHESTRATION PATTERN DEMO                           ║
║                   Payment & Order Service with Compensation                  ║
╚══════════════════════════════════════════════════════════════════════════════╝
");

            // Initialize services
            var orderService = new OrderService();
            var paymentService = new PaymentService();
            var inventoryService = new InventoryService();
            var sagaOrchestrator = new SagaOrchestrator(orderService, paymentService, inventoryService);

            // Display initial state
            DisplayInitialState(paymentService, inventoryService);

            // Wait for user to start
            Console.WriteLine("\nPress ENTER to start Scenario 1: Successful Transaction...");
            Console.ReadLine();

            // Scenario 1: Successful Transaction
            await RunScenario1_SuccessfulTransaction(sagaOrchestrator);

            Console.WriteLine("\n\nPress ENTER to start Scenario 2: Payment Failure (Insufficient Funds)...");
            Console.ReadLine();

            // Scenario 2: Payment Failure - Insufficient Funds
            await RunScenario2_PaymentFailure(sagaOrchestrator);

            Console.WriteLine("\n\nPress ENTER to start Scenario 3: Inventory Failure (Out of Stock)...");
            Console.ReadLine();

            // Scenario 3: Inventory Failure - Out of Stock
            await RunScenario3_InventoryFailure(sagaOrchestrator);

            Console.WriteLine("\n\nPress ENTER to start Scenario 4: Simulated Payment System Failure...");
            Console.ReadLine();

            // Scenario 4: Simulated Payment System Failure
            await RunScenario4_SimulatedFailure(sagaOrchestrator);

            // Display final summary
            DisplayFinalSummary(sagaOrchestrator, orderService, paymentService, inventoryService);

            Console.WriteLine("\n\nPress ENTER to exit...");
            Console.ReadLine();
        }

        static void DisplayInitialState(PaymentService paymentService, InventoryService inventoryService)
        {
            Console.WriteLine("\n╔══════════════════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║                           INITIAL SYSTEM STATE                               ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════════════════════════════╝\n");

            Console.WriteLine("CUSTOMER BALANCES:");
            Console.WriteLine($"  CUST001: ${paymentService.GetCustomerBalance("CUST001"):F2}");
            Console.WriteLine($"  CUST002: ${paymentService.GetCustomerBalance("CUST002"):F2}");
            Console.WriteLine($"  CUST003: ${paymentService.GetCustomerBalance("CUST003"):F2}");

            Console.WriteLine("\nINVENTORY:");
            foreach (var item in inventoryService.GetAllInventory())
            {
                Console.WriteLine($"  {item.ProductId} ({item.ProductName}): {item.AvailableStock} units available");
            }
        }

        static async Task RunScenario1_SuccessfulTransaction(SagaOrchestrator orchestrator)
        {
            Console.WriteLine("\n╔══════════════════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║                    SCENARIO 1: SUCCESSFUL TRANSACTION                        ║");
            Console.WriteLine("║  Customer CUST001 orders 2 Laptops for $2000                                ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════════════════════════════╝");

            var saga = await orchestrator.ExecuteSaga(
                customerId: "CUST001",
                productId: "PROD001",
                quantity: 2,
                amount: 2000m
            );

            DisplaySagaResult(saga);
        }

        static async Task RunScenario2_PaymentFailure(SagaOrchestrator orchestrator)
        {
            Console.WriteLine("\n╔══════════════════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║              SCENARIO 2: PAYMENT FAILURE (INSUFFICIENT FUNDS)                ║");
            Console.WriteLine("║  Customer CUST002 (balance: $50) tries to order Laptop for $1500            ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════════════════════════════╝");

            var saga = await orchestrator.ExecuteSaga(
                customerId: "CUST002",
                productId: "PROD001",
                quantity: 1,
                amount: 1500m
            );

            DisplaySagaResult(saga);
        }

        static async Task RunScenario3_InventoryFailure(SagaOrchestrator orchestrator)
        {
            Console.WriteLine("\n╔══════════════════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║               SCENARIO 3: INVENTORY FAILURE (OUT OF STOCK)                   ║");
            Console.WriteLine("║  Customer CUST003 tries to order 5 Mice (only 2 available)                  ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════════════════════════════╝");

            var saga = await orchestrator.ExecuteSaga(
                customerId: "CUST003",
                productId: "PROD002",
                quantity: 5,
                amount: 150m
            );

            DisplaySagaResult(saga);
        }

        static async Task RunScenario4_SimulatedFailure(SagaOrchestrator orchestrator)
        {
            Console.WriteLine("\n╔══════════════════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║           SCENARIO 4: SIMULATED PAYMENT SYSTEM FAILURE                       ║");
            Console.WriteLine("║  Customer CUST001 orders Keyboard, but payment system fails                 ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════════════════════════════╝");

            var saga = await orchestrator.ExecuteSaga(
                customerId: "CUST001",
                productId: "PROD003",
                quantity: 3,
                amount: 300m,
                simulatePaymentFailure: true
            );

            DisplaySagaResult(saga);
        }

        static void DisplaySagaResult(SagaTransaction saga)
        {
            Console.WriteLine("\n┌──────────────────────────────────────────────────────────────────────────────┐");
            Console.WriteLine("│                           SAGA RESULT SUMMARY                                │");
            Console.WriteLine("└──────────────────────────────────────────────────────────────────────────────┘");
            Console.WriteLine($"Transaction ID: {saga.TransactionId}");
            Console.WriteLine($"Final State:    {saga.State}");
            Console.WriteLine($"Duration:       {(saga.CompletedAt.HasValue ? (saga.CompletedAt.Value - saga.StartedAt).TotalSeconds : 0):F2} seconds");

            if (saga.State == SagaState.Completed)
            {
                Console.WriteLine("\n✓ Transaction completed successfully!");
                Console.WriteLine($"  Executed Steps: {string.Join(" → ", saga.ExecutedSteps)}");
            }
            else if (saga.State == SagaState.Compensated)
            {
                Console.WriteLine($"\n✗ Transaction failed: {saga.ErrorMessage}");
                Console.WriteLine($"  Executed Steps: {string.Join(" → ", saga.ExecutedSteps)}");
                Console.WriteLine($"  Compensated Steps: {string.Join(" → ", saga.CompensatedSteps)}");
            }
        }

        static void DisplayFinalSummary(
            SagaOrchestrator orchestrator,
            OrderService orderService,
            PaymentService paymentService,
            InventoryService inventoryService)
        {
            Console.WriteLine("\n\n╔══════════════════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║                           FINAL SYSTEM STATE                                 ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════════════════════════════╝\n");

            // Transaction Summary
            var allTransactions = orchestrator.GetAllTransactions();
            var successful = allTransactions.Count(t => t.State == SagaState.Completed);
            var failed = allTransactions.Count(t => t.State == SagaState.Compensated);

            Console.WriteLine("TRANSACTION SUMMARY:");
            Console.WriteLine($"  Total Transactions: {allTransactions.Count}");
            Console.WriteLine($"  Successful: {successful}");
            Console.WriteLine($"  Failed (Compensated): {failed}");

            // Customer Balances
            Console.WriteLine("\nFINAL CUSTOMER BALANCES:");
            Console.WriteLine($"  CUST001: ${paymentService.GetCustomerBalance("CUST001"):F2}");
            Console.WriteLine($"  CUST002: ${paymentService.GetCustomerBalance("CUST002"):F2}");
            Console.WriteLine($"  CUST003: ${paymentService.GetCustomerBalance("CUST003"):F2}");

            // Inventory Status
            Console.WriteLine("\nFINAL INVENTORY:");
            foreach (var item in inventoryService.GetAllInventory())
            {
                Console.WriteLine($"  {item.ProductId} ({item.ProductName}):");
                Console.WriteLine($"    Available: {item.AvailableStock}, Reserved: {item.ReservedStock}");
            }

            // Order Status
            Console.WriteLine("\nORDER STATUS:");
            foreach (var order in orderService.GetAllOrders())
            {
                Console.WriteLine($"  {order.OrderId}: {order.Status}");
            }
        }
    }
}
