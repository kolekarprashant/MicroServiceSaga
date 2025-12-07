namespace SagaPattern.Models
{
    public enum OrderStatus
    {
        Pending,
        Confirmed,
        Cancelled,
        Failed
    }

    public enum PaymentStatus
    {
        Pending,
        Success,
        Failed,
        Refunded
    }

    public enum InventoryStatus
    {
        Available,
        Reserved,
        Released,
        OutOfStock
    }

    public enum SagaState
    {
        Started,
        OrderCreated,
        InventoryReserved,
        PaymentProcessed,
        Completed,
        Failed,
        Compensating,
        Compensated
    }
}
