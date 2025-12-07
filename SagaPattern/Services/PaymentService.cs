using SagaPattern.Models;

namespace SagaPattern.Services
{
    public class PaymentService
    {
        private readonly Dictionary<string, Payment> _payments = new();
        private readonly Dictionary<string, decimal> _customerBalances = new();
        private readonly object _lock = new();
        private readonly Random _random = new();

        public PaymentService()
        {
            // Initialize some customer balances for demo
            _customerBalances["CUST001"] = 1000m;
            _customerBalances["CUST002"] = 50m;  // Low balance for testing failures
            _customerBalances["CUST003"] = 5000m;
        }

        public Payment ProcessPayment(string orderId, string customerId, decimal amount, bool simulateFailure = false)
        {
            lock (_lock)
            {
                var payment = new Payment
                {
                    OrderId = orderId,
                    CustomerId = customerId,
                    Amount = amount,
                    Status = PaymentStatus.Pending
                };

                _payments[payment.PaymentId] = payment;

                Console.WriteLine($"[PaymentService] Processing payment: {payment.PaymentId}");
                Console.WriteLine($"  → Order: {orderId}, Customer: {customerId}, Amount: ${amount}");

                // Simulate payment processing delay
                Thread.Sleep(500);

                // Check if we should simulate a failure
                if (simulateFailure)
                {
                    payment.Status = PaymentStatus.Failed;
                    payment.FailureReason = "Simulated payment failure for testing";
                    Console.WriteLine($"[PaymentService] Payment FAILED: {payment.PaymentId}");
                    Console.WriteLine($"  → Reason: {payment.FailureReason}");
                    return payment;
                }

                // Check customer balance
                if (!_customerBalances.ContainsKey(customerId))
                {
                    payment.Status = PaymentStatus.Failed;
                    payment.FailureReason = "Customer not found";
                    Console.WriteLine($"[PaymentService] Payment FAILED: {payment.PaymentId}");
                    Console.WriteLine($"  → Reason: {payment.FailureReason}");
                    return payment;
                }

                var balance = _customerBalances[customerId];
                if (balance < amount)
                {
                    payment.Status = PaymentStatus.Failed;
                    payment.FailureReason = $"Insufficient funds. Balance: ${balance}, Required: ${amount}";
                    Console.WriteLine($"[PaymentService] Payment FAILED: {payment.PaymentId}");
                    Console.WriteLine($"  → Reason: {payment.FailureReason}");
                    return payment;
                }

                // Process successful payment
                _customerBalances[customerId] -= amount;
                payment.Status = PaymentStatus.Success;

                Console.WriteLine($"[PaymentService] Payment SUCCESS: {payment.PaymentId}");
                Console.WriteLine($"  → Remaining balance: ${_customerBalances[customerId]}");

                return payment;
            }
        }

        public bool RefundPayment(string paymentId)
        {
            lock (_lock)
            {
                if (!_payments.TryGetValue(paymentId, out var payment))
                {
                    Console.WriteLine($"[PaymentService] Payment not found for refund: {paymentId}");
                    return false;
                }

                if (payment.Status != PaymentStatus.Success)
                {
                    Console.WriteLine($"[PaymentService] Cannot refund payment with status: {payment.Status}");
                    return false;
                }

                // Refund the amount
                if (_customerBalances.ContainsKey(payment.CustomerId))
                {
                    _customerBalances[payment.CustomerId] += payment.Amount;
                }

                payment.Status = PaymentStatus.Refunded;

                Console.WriteLine($"[PaymentService] Payment refunded: {paymentId}");
                Console.WriteLine($"  → Amount: ${payment.Amount} returned to customer {payment.CustomerId}");

                return true;
            }
        }

        public Payment? GetPayment(string paymentId)
        {
            lock (_lock)
            {
                return _payments.TryGetValue(paymentId, out var payment) ? payment : null;
            }
        }

        public decimal GetCustomerBalance(string customerId)
        {
            lock (_lock)
            {
                return _customerBalances.TryGetValue(customerId, out var balance) ? balance : 0;
            }
        }
    }
}
