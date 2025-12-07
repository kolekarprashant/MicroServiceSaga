namespace Saga.Api.Models
{
    public enum SagaState
    {
        Started,
        OrderCreated,
        PaymentProcessed,
        Completed,
        Failed,
        Compensating,
        Compensated
    }

    public class SagaTransaction
    {
        public string TransactionId { get; set; } = string.Empty;
        public string OrderId { get; set; } = string.Empty;
        public string? PaymentId { get; set; }
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
