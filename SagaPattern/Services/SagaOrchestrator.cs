using SagaPattern.Models;

namespace SagaPattern.Services
{
    public class SagaOrchestrator
    {
        private readonly OrderService _orderService;
        private readonly PaymentService _paymentService;
        private readonly InventoryService _inventoryService;
        private readonly Dictionary<string, SagaTransaction> _transactions = new();
        private readonly object _lock = new();

        public SagaOrchestrator(
            OrderService orderService,
            PaymentService paymentService,
            InventoryService inventoryService)
        {
            _orderService = orderService;
            _paymentService = paymentService;
            _inventoryService = inventoryService;
        }

        public async Task<SagaTransaction> ExecuteSaga(
            string customerId,
            string productId,
            int quantity,
            decimal amount,
            bool simulatePaymentFailure = false,
            bool simulateInventoryFailure = false)
        {
            var saga = new SagaTransaction();
            _transactions[saga.TransactionId] = saga;

            Console.WriteLine("\n" + new string('=', 80));
            Console.WriteLine($"SAGA TRANSACTION STARTED: {saga.TransactionId}");
            Console.WriteLine(new string('=', 80));

            try
            {
                // Step 1: Create Order
                Console.WriteLine("\n--- STEP 1: CREATE ORDER ---");
                var order = _orderService.CreateOrder(customerId, productId, quantity, amount);
                saga.OrderId = order.OrderId;
                saga.State = SagaState.OrderCreated;
                saga.ExecutedSteps.Add("OrderCreated");

                // Step 2: Reserve Inventory
                Console.WriteLine("\n--- STEP 2: RESERVE INVENTORY ---");
                var reservation = _inventoryService.ReserveStock(
                    order.OrderId,
                    productId,
                    quantity,
                    simulateInventoryFailure);

                if (reservation == null)
                {
                    saga.ErrorMessage = "Failed to reserve inventory";
                    saga.State = SagaState.Failed;
                    await CompensateSaga(saga);
                    return saga;
                }

                saga.ReservationId = reservation.ReservationId;
                saga.State = SagaState.InventoryReserved;
                saga.ExecutedSteps.Add("InventoryReserved");

                // Step 3: Process Payment
                Console.WriteLine("\n--- STEP 3: PROCESS PAYMENT ---");
                var payment = _paymentService.ProcessPayment(
                    order.OrderId,
                    customerId,
                    amount,
                    simulatePaymentFailure);

                saga.PaymentId = payment.PaymentId;

                if (payment.Status != PaymentStatus.Success)
                {
                    saga.ErrorMessage = $"Payment failed: {payment.FailureReason}";
                    saga.State = SagaState.Failed;
                    await CompensateSaga(saga);
                    return saga;
                }

                saga.State = SagaState.PaymentProcessed;
                saga.ExecutedSteps.Add("PaymentProcessed");

                // Step 4: Confirm Order and Inventory
                Console.WriteLine("\n--- STEP 4: CONFIRM ORDER ---");
                _orderService.ConfirmOrder(order.OrderId);
                _inventoryService.ConfirmReservation(reservation.ReservationId);

                saga.State = SagaState.Completed;
                saga.CompletedAt = DateTime.UtcNow;

                Console.WriteLine("\n" + new string('=', 80));
                Console.WriteLine("SAGA TRANSACTION COMPLETED SUCCESSFULLY");
                Console.WriteLine($"Transaction ID: {saga.TransactionId}");
                Console.WriteLine($"Order ID: {saga.OrderId}");
                Console.WriteLine($"Duration: {(saga.CompletedAt.Value - saga.StartedAt).TotalSeconds:F2} seconds");
                Console.WriteLine(new string('=', 80));

                return saga;
            }
            catch (Exception ex)
            {
                saga.ErrorMessage = ex.Message;
                saga.State = SagaState.Failed;
                await CompensateSaga(saga);
                return saga;
            }
        }

        private async Task CompensateSaga(SagaTransaction saga)
        {
            Console.WriteLine("\n" + new string('!', 80));
            Console.WriteLine("SAGA TRANSACTION FAILED - STARTING COMPENSATION");
            Console.WriteLine($"Error: {saga.ErrorMessage}");
            Console.WriteLine(new string('!', 80));

            saga.State = SagaState.Compensating;

            // Compensate in reverse order of execution
            var stepsToCompensate = new List<string>(saga.ExecutedSteps);
            stepsToCompensate.Reverse();

            foreach (var step in stepsToCompensate)
            {
                Console.WriteLine($"\n--- COMPENSATING: {step} ---");

                switch (step)
                {
                    case "PaymentProcessed":
                        if (!string.IsNullOrEmpty(saga.PaymentId))
                        {
                            _paymentService.RefundPayment(saga.PaymentId);
                            saga.CompensatedSteps.Add("PaymentRefunded");
                        }
                        break;

                    case "InventoryReserved":
                        if (!string.IsNullOrEmpty(saga.ReservationId))
                        {
                            _inventoryService.ReleaseReservation(saga.ReservationId);
                            saga.CompensatedSteps.Add("InventoryReleased");
                        }
                        break;

                    case "OrderCreated":
                        if (!string.IsNullOrEmpty(saga.OrderId))
                        {
                            _orderService.CancelOrder(saga.OrderId, saga.ErrorMessage ?? "Saga failed");
                            saga.CompensatedSteps.Add("OrderCancelled");
                        }
                        break;
                }

                // Simulate compensation delay
                await Task.Delay(200);
            }

            saga.State = SagaState.Compensated;
            saga.CompletedAt = DateTime.UtcNow;

            Console.WriteLine("\n" + new string('!', 80));
            Console.WriteLine("SAGA COMPENSATION COMPLETED");
            Console.WriteLine($"Transaction ID: {saga.TransactionId}");
            Console.WriteLine($"Steps Compensated: {saga.CompensatedSteps.Count}");
            Console.WriteLine(new string('!', 80));
        }

        public SagaTransaction? GetTransaction(string transactionId)
        {
            lock (_lock)
            {
                return _transactions.TryGetValue(transactionId, out var saga) ? saga : null;
            }
        }

        public List<SagaTransaction> GetAllTransactions()
        {
            lock (_lock)
            {
                return _transactions.Values.ToList();
            }
        }
    }
}
