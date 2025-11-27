using MassTransit;

namespace MassTransitSagaDemo;

public class OrderState : SagaStateMachineInstance
{
    public Guid CorrelationId { get; set; }
    public string? CurrentState { get; set; }
    public bool InventoryReserved { get; set; }
    public bool PaymentProcessed { get; set; }
    public bool InventoryReleased { get; set; }
    public DateTime? Created { get; set; }
    public decimal Amount { get; set; }
}
