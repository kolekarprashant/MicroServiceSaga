using System.Collections.Concurrent;

namespace OrderService;

public class OrderTrackingService
{
    private readonly ConcurrentDictionary<Guid, OrderStatus> _orders = new();

    public void AddOrder(Guid orderId, decimal amount, string productId, int quantity)
    {
        _orders[orderId] = new OrderStatus
        {
            OrderId = orderId,
            Amount = amount,
            ProductId = productId,
            Quantity = quantity,
            Status = "Submitted",
            SubmittedAt = DateTime.UtcNow
        };
    }

    public void UpdateOrderStatus(Guid orderId, string status, string? reason = null)
    {
        if (_orders.TryGetValue(orderId, out var order))
        {
            order.Status = status;
            order.Reason = reason;
            order.UpdatedAt = DateTime.UtcNow;
        }
    }

    public OrderStatus? GetOrderStatus(Guid orderId)
    {
        _orders.TryGetValue(orderId, out var order);
        return order;
    }

    public List<OrderStatus> GetAllOrders()
    {
        return _orders.Values.OrderByDescending(o => o.SubmittedAt).ToList();
    }
}

public class OrderStatus
{
    public Guid OrderId { get; set; }
    public decimal Amount { get; set; }
    public string ProductId { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Reason { get; set; }
    public DateTime SubmittedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
