namespace SagaPattern.Models
{
    public class Order
    {
        public string OrderId { get; set; } = string.Empty;
        public string CustomerId { get; set; } = string.Empty;
        public string ProductId { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal Amount { get; set; }
        public OrderStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public Order()
        {
            OrderId = Guid.NewGuid().ToString();
            CreatedAt = DateTime.UtcNow;
            Status = OrderStatus.Pending;
        }
    }
}
