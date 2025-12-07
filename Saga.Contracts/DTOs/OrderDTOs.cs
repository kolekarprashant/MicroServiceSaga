namespace Saga.Contracts.DTOs
{
    public class CreateOrderRequest
    {
        public string CustomerId { get; set; } = string.Empty;
        public string ProductId { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal Amount { get; set; }
    }

    public class OrderResponse
    {
        public string OrderId { get; set; } = string.Empty;
        public string CustomerId { get; set; } = string.Empty;
        public string ProductId { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal Amount { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class OrderStatusUpdateRequest
    {
        public string OrderId { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? Reason { get; set; }
    }
}
