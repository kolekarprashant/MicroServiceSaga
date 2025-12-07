using MassTransit;
using MassTransit.Messages;

namespace InventoryService;

public class ReserveInventoryConsumer : IConsumer<ReserveInventory>
{
    private readonly ILogger<ReserveInventoryConsumer> _logger;
    private readonly InventoryTrackingService _inventoryTracking;

    public ReserveInventoryConsumer(ILogger<ReserveInventoryConsumer> logger, InventoryTrackingService inventoryTracking)
    {
        _logger = logger;
        _inventoryTracking = inventoryTracking;
    }

    public async Task Consume(ConsumeContext<ReserveInventory> context)
    {
        _logger.LogInformation("📦 InventoryService: Reserving inventory for Order {OrderId} - Product: {ProductId}, Qty: {Quantity}", 
            context.Message.OrderId, context.Message.ProductId, context.Message.Quantity);

        // Simulate inventory check and reservation
        await Task.Delay(300);

        _inventoryTracking.AddReservation(context.Message.OrderId, context.Message.ProductId, context.Message.Quantity);

        _logger.LogInformation("📦 InventoryService: Inventory reserved successfully for Order {OrderId}", 
            context.Message.OrderId);

        await context.Publish(new InventoryReserved 
        { 
            OrderId = context.Message.OrderId 
        });
    }
}

public class ReleaseInventoryConsumer : IConsumer<ReleaseInventory>
{
    private readonly ILogger<ReleaseInventoryConsumer> _logger;
    private readonly InventoryTrackingService _inventoryTracking;

    public ReleaseInventoryConsumer(ILogger<ReleaseInventoryConsumer> logger, InventoryTrackingService inventoryTracking)
    {
        _logger = logger;
        _inventoryTracking = inventoryTracking;
    }

    public async Task Consume(ConsumeContext<ReleaseInventory> context)
    {
        _logger.LogInformation("📦 InventoryService: Releasing inventory for Order {OrderId} (Compensation)", 
            context.Message.OrderId);

        // Simulate inventory release
        await Task.Delay(200);

        _inventoryTracking.ReleaseReservation(context.Message.OrderId);

        _logger.LogInformation("📦 InventoryService: Inventory released for Order {OrderId}", 
            context.Message.OrderId);

        await context.Publish(new InventoryReleased 
        { 
            OrderId = context.Message.OrderId 
        });
    }
}
