namespace SagaPattern.Models
{
    public class SagaTransaction
    {
        public string TransactionId { get; set; } = string.Empty;
        public string OrderId { get; set; } = string.Empty;
        public string? PaymentId { get; set; }
        public string? ReservationId { get; set; }
        public SagaState State { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public List<string> ExecutedSteps { get; set; } = new();
        public List<string> CompensatedSteps { get; set; } = new();
        public string? ErrorMessage { get; set; }

        public SagaTransaction()
        {
            TransactionId = Guid.NewGuid().ToString();
            StartedAt = DateTime.UtcNow;
            State = SagaState.Started;
        }
    }
}
