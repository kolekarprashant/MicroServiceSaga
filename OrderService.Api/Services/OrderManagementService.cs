using OrderService.Api.Models;

namespace OrderService.Api.Services
{
    public class OrderManagementService
    {
        private readonly Dictionary<string, Order> _orders = new();
        private readonly object _lock = new();
        private readonly ILogger<OrderManagementService> _logger;

        public OrderManagementService(ILogger<OrderManagementService> logger)
        {
            _logger = logger;
        }

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

                _logger.LogInformation(
                    "Order created: {OrderId} for customer {CustomerId}, Product: {ProductId}, Quantity: {Quantity}, Amount: {Amount}",
                    order.OrderId, customerId, productId, quantity, amount);

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

                    _logger.LogInformation("Order confirmed: {OrderId}", orderId);
                    return true;
                }

                _logger.LogWarning("Order not found for confirmation: {OrderId}", orderId);
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

                    _logger.LogInformation("Order cancelled: {OrderId}, Reason: {Reason}", orderId, reason);
                    return true;
                }

                _logger.LogWarning("Order not found for cancellation: {OrderId}", orderId);
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

                    _logger.LogInformation("Order marked as failed: {OrderId}, Reason: {Reason}", orderId, reason);
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

        public List<Order> GetOrdersByCustomer(string customerId)
        {
            lock (_lock)
            {
                return _orders.Values.Where(o => o.CustomerId == customerId).ToList();
            }
        }
    }
}
