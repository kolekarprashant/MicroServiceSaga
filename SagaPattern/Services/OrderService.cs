using SagaPattern.Models;

namespace SagaPattern.Services
{
    public class OrderService
    {
        private readonly Dictionary<string, Order> _orders = new();
        private readonly object _lock = new();

        public Order CreateOrder(string customerId, string productId, int quantity, decimal amount)
        {
            lock (_lock)
            {
                var order = new Order
                {
                    CustomerId = customerId,
                    ProductId = productId,
                    Quantity = quantity,
                    Amount = amount,
                    Status = OrderStatus.Pending
                };

                _orders[order.OrderId] = order;

                Console.WriteLine($"[OrderService] Order created: {order.OrderId} for customer {customerId}");
                Console.WriteLine($"  → Product: {productId}, Quantity: {quantity}, Amount: ${amount}");

                return order;
            }
        }

        public bool ConfirmOrder(string orderId)
        {
            lock (_lock)
            {
                if (_orders.TryGetValue(orderId, out var order))
                {
                    order.Status = OrderStatus.Confirmed;
                    order.UpdatedAt = DateTime.UtcNow;

                    Console.WriteLine($"[OrderService] Order confirmed: {orderId}");
                    return true;
                }

                Console.WriteLine($"[OrderService] Order not found: {orderId}");
                return false;
            }
        }

        public bool CancelOrder(string orderId, string reason)
        {
            lock (_lock)
            {
                if (_orders.TryGetValue(orderId, out var order))
                {
                    order.Status = OrderStatus.Cancelled;
                    order.UpdatedAt = DateTime.UtcNow;

                    Console.WriteLine($"[OrderService] Order cancelled: {orderId}");
                    Console.WriteLine($"  → Reason: {reason}");
                    return true;
                }

                Console.WriteLine($"[OrderService] Order not found: {orderId}");
                return false;
            }
        }

        public bool MarkOrderFailed(string orderId, string reason)
        {
            lock (_lock)
            {
                if (_orders.TryGetValue(orderId, out var order))
                {
                    order.Status = OrderStatus.Failed;
                    order.UpdatedAt = DateTime.UtcNow;

                    Console.WriteLine($"[OrderService] Order failed: {orderId}");
                    Console.WriteLine($"  → Reason: {reason}");
                    return true;
                }

                return false;
            }
        }

        public Order? GetOrder(string orderId)
        {
            lock (_lock)
            {
                return _orders.TryGetValue(orderId, out var order) ? order : null;
            }
        }

        public List<Order> GetAllOrders()
        {
            lock (_lock)
            {
                return _orders.Values.ToList();
            }
        }
    }
}
