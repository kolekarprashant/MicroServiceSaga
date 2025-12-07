using MassTransit;
using MassTransit.Messages;
using Microsoft.AspNetCore.Mvc;

namespace OrderService;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IBus _bus;
    private readonly ILogger<OrdersController> _logger;
    private readonly OrderTrackingService _orderTracking;

    public OrdersController(IBus bus, ILogger<OrdersController> logger, OrderTrackingService orderTracking)
    {
        _bus = bus;
        _logger = logger;
        _orderTracking = orderTracking;
    }

    [HttpPost]
    public async Task<IActionResult> SubmitOrder([FromBody] CreateOrderRequest request)
    {
        var orderId = Guid.NewGuid();
        
        var order = new SubmitOrder
        {
            OrderId = orderId,
            Amount = request.Amount,
            ProductId = request.ProductId,
            Quantity = request.Quantity
        };

        _logger.LogInformation("🛒 OrderService API: Submitting order {OrderId} for ${Amount} ({ProductId} x {Quantity})", 
            order.OrderId, order.Amount, order.ProductId, order.Quantity);

        _orderTracking.AddOrder(orderId, order.Amount, order.ProductId, order.Quantity);

        await _bus.Publish(order);

        return Ok(new 
        { 
            orderId = orderId, 
            message = "Order submitted successfully",
            amount = order.Amount,
            productId = order.ProductId,
            quantity = order.Quantity
        });
    }

    [HttpGet("{orderId}")]
    public IActionResult GetOrderStatus(Guid orderId)
    {
        var status = _orderTracking.GetOrderStatus(orderId);
        if (status == null)
        {
            return NotFound(new { message = "Order not found" });
        }

        return Ok(status);
    }

    [HttpGet]
    public IActionResult GetAllOrders()
    {
        var orders = _orderTracking.GetAllOrders();
        return Ok(orders);
    }
}

public record CreateOrderRequest
{
    public decimal Amount { get; init; }
    public string ProductId { get; init; } = string.Empty;
    public int Quantity { get; init; }
}
