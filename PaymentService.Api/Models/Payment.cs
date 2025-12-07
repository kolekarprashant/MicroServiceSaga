namespace PaymentService.Api.Models
{
    public enum PaymentStatus
    {
        Pending,
        Success,
        Failed,
        Refunded
    }

    public class Payment
    {
        public string PaymentId { get; set; } = string.Empty;
        public string OrderId { get; set; } = string.Empty;
        public string CustomerId { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public PaymentStatus Status { get; set; }
        public DateTime ProcessedAt { get; set; }
        public string? FailureReason { get; set; }

        public Payment()
        {
            PaymentId = Guid.NewGuid().ToString();
            ProcessedAt = DateTime.UtcNow;
            Status = PaymentStatus.Pending;
        }
    }
}
