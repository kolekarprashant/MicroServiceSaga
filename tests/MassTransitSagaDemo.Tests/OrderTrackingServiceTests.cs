using OrderService;
using MassTransit.Messages;

namespace MassTransitSagaDemo.Tests;

public class OrderTrackingServiceTests
{
    [Fact]
    public void Should_Add_Order_Successfully()
    {
        var service = new OrderTrackingService();
        var orderId = Guid.NewGuid();

        service.AddOrder(orderId, 100.50m, "PROD-123", 5);
        var order = service.GetOrderStatus(orderId);

        Assert.NotNull(order);
        Assert.Equal(orderId, order.OrderId);
        Assert.Equal("PROD-123", order.ProductId);
        Assert.Equal(5, order.Quantity);
        Assert.Equal(100.50m, order.Amount);
        Assert.Equal("Submitted", order.Status);
    }

    [Fact]
    public void Should_Update_Order_Status()
    {
        var service = new OrderTrackingService();
        var orderId = Guid.NewGuid();

        service.AddOrder(orderId, 100.50m, "PROD-123", 5);
        service.UpdateOrderStatus(orderId, "Completed");
        
        var order = service.GetOrderStatus(orderId);
        Assert.Equal("Completed", order.Status);
    }

    [Fact]
    public void Should_Return_Null_For_NonExistent_Order()
    {
        var service = new OrderTrackingService();
        var order = service.GetOrderStatus(Guid.NewGuid());

        Assert.Null(order);
    }

    [Fact]
    public void Should_Return_All_Orders()
    {
        var service = new OrderTrackingService();
        
        service.AddOrder(Guid.NewGuid(), 100m, "PROD-1", 1);
        service.AddOrder(Guid.NewGuid(), 200m, "PROD-2", 2);
        service.AddOrder(Guid.NewGuid(), 300m, "PROD-3", 3);

        var orders = service.GetAllOrders();
        Assert.Equal(3, orders.Count);
    }

    [Fact]
    public void Should_Handle_Multiple_Status_Updates()
    {
        var service = new OrderTrackingService();
        var orderId = Guid.NewGuid();

        service.AddOrder(orderId, 100.50m, "PROD-123", 5);
        service.UpdateOrderStatus(orderId, "Processing");
        service.UpdateOrderStatus(orderId, "Completed");
        
        var order = service.GetOrderStatus(orderId);
        Assert.Equal("Completed", order.Status);
    }
}
