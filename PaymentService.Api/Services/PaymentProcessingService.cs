using PaymentService.Api.Models;

namespace PaymentService.Api.Services
{
    public class PaymentProcessingService
    {
        private readonly Dictionary<string, Payment> _payments = new();
        private readonly Dictionary<string, decimal> _customerBalances = new();
        private readonly object _lock = new();
        private readonly ILogger<PaymentProcessingService> _logger;

        public PaymentProcessingService(ILogger<PaymentProcessingService> logger)
        {
            _logger = logger;

            // Initialize customer balances for demo
            _customerBalances["CUST001"] = 10000m;
            _customerBalances["CUST002"] = 50m;
            _customerBalances["CUST003"] = 5000m;
            _customerBalances["CUST004"] = 20000m;
        }

        public Payment ProcessPayment(string orderId, string customerId, decimal amount)
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

                _logger.LogInformation(
                    "Processing payment: {PaymentId}, Order: {OrderId}, Customer: {CustomerId}, Amount: {Amount}",
                    payment.PaymentId, orderId, customerId, amount);

                // Simulate processing delay
                Thread.Sleep(300);

                // Check customer exists
                if (!_customerBalances.ContainsKey(customerId))
                {
                    payment.Status = PaymentStatus.Failed;
                    payment.FailureReason = "Customer not found";
                    _logger.LogWarning("Payment failed: Customer not found - {CustomerId}", customerId);
                    return payment;
                }

                // Check balance
                var balance = _customerBalances[customerId];
                if (balance < amount)
                {
                    payment.Status = PaymentStatus.Failed;
                    payment.FailureReason = $"Insufficient funds. Balance: ${balance}, Required: ${amount}";
                    _logger.LogWarning(
                        "Payment failed: Insufficient funds - {CustomerId}, Balance: {Balance}, Required: {Amount}",
                        customerId, balance, amount);
                    return payment;
                }

                // Process successful payment
                _customerBalances[customerId] -= amount;
                payment.Status = PaymentStatus.Success;

                _logger.LogInformation(
                    "Payment successful: {PaymentId}, Remaining balance: {Balance}",
                    payment.PaymentId, _customerBalances[customerId]);

                return payment;
            }
        }

        public bool RefundPayment(string paymentId)
        {
            lock (_lock)
            {
                if (!_payments.TryGetValue(paymentId, out var payment))
                {
                    _logger.LogWarning("Payment not found for refund: {PaymentId}", paymentId);
                    return false;
                }

                if (payment.Status != PaymentStatus.Success)
                {
                    _logger.LogWarning(
                        "Cannot refund payment with status: {Status}, PaymentId: {PaymentId}",
                        payment.Status, paymentId);
                    return false;
                }

                // Refund the amount
                if (_customerBalances.ContainsKey(payment.CustomerId))
                {
                    _customerBalances[payment.CustomerId] += payment.Amount;
                }

                payment.Status = PaymentStatus.Refunded;

                _logger.LogInformation(
                    "Payment refunded: {PaymentId}, Amount: {Amount}, Customer: {CustomerId}",
                    paymentId, payment.Amount, payment.CustomerId);

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

        public List<Payment> GetAllPayments()
        {
            lock (_lock)
            {
                return _payments.Values.ToList();
            }
        }

        public List<Payment> GetPaymentsByCustomer(string customerId)
        {
            lock (_lock)
            {
                return _payments.Values.Where(p => p.CustomerId == customerId).ToList();
            }
        }
    }
}
