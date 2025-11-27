using MassTransit;
using MassTransit.Messages;

namespace OrderService;

public class OrderCompletedConsumer : IConsumer<OrderCompleted>
{
    private readonly ILogger<OrderCompletedConsumer> _logger;
    private readonly OrderTrackingService _orderTracking;

    public OrderCompletedConsumer(ILogger<OrderCompletedConsumer> logger, OrderTrackingService orderTracking)
    {
        _logger = logger;
        _orderTracking = orderTracking;
    }

    public Task Consume(ConsumeContext<OrderCompleted> context)
    {
        _logger.LogInformation("✅ OrderService: Order {OrderId} completed successfully! Amount: ${Amount}", 
            context.Message.OrderId, context.Message.Amount);
        
        _orderTracking.UpdateOrderStatus(context.Message.OrderId, "Completed");
        
        return Task.CompletedTask;
    }
}

public class OrderFailedConsumer : IConsumer<OrderFailed>
{
    private readonly ILogger<OrderFailedConsumer> _logger;
    private readonly OrderTrackingService _orderTracking;

    public OrderFailedConsumer(ILogger<OrderFailedConsumer> logger, OrderTrackingService orderTracking)
    {
        _logger = logger;
        _orderTracking = orderTracking;
    }

    public Task Consume(ConsumeContext<OrderFailed> context)
    {
        _logger.LogError("❌ OrderService: Order {OrderId} failed! Reason: {Reason}", 
            context.Message.OrderId, context.Message.Reason);
        
        _orderTracking.UpdateOrderStatus(context.Message.OrderId, "Failed", context.Message.Reason);
        
        return Task.CompletedTask;
    }
}
