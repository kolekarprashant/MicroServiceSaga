namespace Saga.Contracts.DTOs
{
    public class ProcessPaymentRequest
    {
        public string OrderId { get; set; } = string.Empty;
        public string CustomerId { get; set; } = string.Empty;
        public decimal Amount { get; set; }
    }

    public class PaymentResponse
    {
        public string PaymentId { get; set; } = string.Empty;
        public string OrderId { get; set; } = string.Empty;
        public string CustomerId { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime ProcessedAt { get; set; }
        public string? FailureReason { get; set; }
    }

    public class RefundPaymentRequest
    {
        public string PaymentId { get; set; } = string.Empty;
    }

    public class CustomerBalanceResponse
    {
        public string CustomerId { get; set; } = string.Empty;
        public decimal Balance { get; set; }
    }
}
