namespace Saga.Contracts.DTOs
{
    public class ExecuteSagaRequest
    {
        public string CustomerId { get; set; } = string.Empty;
        public string ProductId { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal Amount { get; set; }
    }

    public class SagaTransactionResponse
    {
        public string TransactionId { get; set; } = string.Empty;
        public string OrderId { get; set; } = string.Empty;
        public string? PaymentId { get; set; }
        public string? ReservationId { get; set; }
        public string State { get; set; } = string.Empty;
        public DateTime StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public List<string> ExecutedSteps { get; set; } = new();
        public List<string> CompensatedSteps { get; set; } = new();
        public string? ErrorMessage { get; set; }
        public double? DurationSeconds { get; set; }
    }
}
