namespace MassTransit.Messages;

// Commands / Events
public record SubmitOrder
{
    public Guid OrderId { get; init; }
    public decimal Amount { get; init; }
    public string ProductId { get; init; } = string.Empty;
    public int Quantity { get; init; }
}

public record ReserveInventory
{
    public Guid OrderId { get; init; }
    public string ProductId { get; init; } = string.Empty;
    public int Quantity { get; init; }
}

public record InventoryReserved
{
    public Guid OrderId { get; init; }
}

public record ProcessPayment
{
    public Guid OrderId { get; init; }
    public decimal Amount { get; init; }
}

public record PaymentProcessed
{
    public Guid OrderId { get; init; }
}

public record PaymentFailed
{
    public Guid OrderId { get; init; }
    public string Reason { get; init; } = string.Empty;
}

public record ReleaseInventory
{
    public Guid OrderId { get; init; }
}

public record InventoryReleased
{
    public Guid OrderId { get; init; }
}

public record OrderCompleted
{
    public Guid OrderId { get; init; }
    public decimal Amount { get; init; }
}

public record OrderFailed
{
    public Guid OrderId { get; init; }
    public string Reason { get; init; } = string.Empty;
}
