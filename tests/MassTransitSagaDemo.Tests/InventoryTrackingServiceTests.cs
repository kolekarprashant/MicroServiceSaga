using InventoryService;

namespace MassTransitSagaDemo.Tests;

public class InventoryTrackingServiceTests
{
    [Fact]
    public void Should_Reserve_Inventory_Successfully()
    {
        var service = new InventoryTrackingService();
        var orderId = Guid.NewGuid();

        service.AddReservation(orderId, "PROD-123", 5);
        var record = service.GetInventoryByOrder(orderId);

        Assert.NotNull(record);
        Assert.Equal(orderId, record.OrderId);
        Assert.Equal("PROD-123", record.ProductId);
        Assert.Equal(5, record.Quantity);
        Assert.Equal("Reserved", record.Status);
        Assert.True(record.ReservedAt <= DateTime.UtcNow);
    }

    [Fact]
    public void Should_Release_Inventory_Successfully()
    {
        var service = new InventoryTrackingService();
        var orderId = Guid.NewGuid();

        service.AddReservation(orderId, "PROD-123", 5);
        service.ReleaseReservation(orderId);
        
        var record = service.GetInventoryByOrder(orderId);
        Assert.Equal("Released", record.Status);
        Assert.NotNull(record.ReleasedAt);
    }

    [Fact]
    public void Should_Return_Null_For_NonExistent_Inventory()
    {
        var service = new InventoryTrackingService();
        var record = service.GetInventoryByOrder(Guid.NewGuid());

        Assert.Null(record);
    }

    [Fact]
    public void Should_Not_Throw_When_Releasing_NonExistent_Inventory()
    {
        var service = new InventoryTrackingService();
        var exception = Record.Exception(() => service.ReleaseReservation(Guid.NewGuid()));

        Assert.Null(exception);
    }

    [Fact]
    public void Should_Get_All_Inventory_Records()
    {
        var service = new InventoryTrackingService();
        
        service.AddReservation(Guid.NewGuid(), "PROD-1", 1);
        service.AddReservation(Guid.NewGuid(), "PROD-2", 2);
        service.AddReservation(Guid.NewGuid(), "PROD-3", 3);

        var records = service.GetAllInventoryRecords();
        Assert.Equal(3, records.Count);
    }

    [Fact]
    public void Should_Track_Multiple_Reserve_And_Release_Operations()
    {
        var service = new InventoryTrackingService();
        var orderId1 = Guid.NewGuid();
        var orderId2 = Guid.NewGuid();

        service.AddReservation(orderId1, "PROD-1", 1);
        service.AddReservation(orderId2, "PROD-2", 2);
        service.ReleaseReservation(orderId1);

        var record1 = service.GetInventoryByOrder(orderId1);
        var record2 = service.GetInventoryByOrder(orderId2);

        Assert.Equal("Released", record1.Status);
        Assert.Equal("Reserved", record2.Status);
    }

    [Fact]
    public void Should_Set_ReservedAt_Timestamp()
    {
        var service = new InventoryTrackingService();
        var orderId = Guid.NewGuid();
        var beforeReserve = DateTime.UtcNow;

        service.AddReservation(orderId, "PROD-123", 5);
        var record = service.GetInventoryByOrder(orderId);

        Assert.NotNull(record.ReservedAt);
        Assert.True(record.ReservedAt >= beforeReserve);
        Assert.True(record.ReservedAt <= DateTime.UtcNow);
    }

    [Fact]
    public void Should_Set_ReleasedAt_Timestamp()
    {
        var service = new InventoryTrackingService();
        var orderId = Guid.NewGuid();

        service.AddReservation(orderId, "PROD-123", 5);
        var beforeRelease = DateTime.UtcNow;
        service.ReleaseReservation(orderId);
        
        var record = service.GetInventoryByOrder(orderId);

        Assert.NotNull(record.ReleasedAt);
        Assert.True(record.ReleasedAt >= beforeRelease);
        Assert.True(record.ReleasedAt <= DateTime.UtcNow);
    }
    
    [Fact]
    public void Should_Get_Inventory_By_Product()
    {
        var service = new InventoryTrackingService();
        
        service.AddReservation(Guid.NewGuid(), "PROD-1", 1);
        service.AddReservation(Guid.NewGuid(), "PROD-1", 2);
        service.AddReservation(Guid.NewGuid(), "PROD-2", 3);

        var records = service.GetInventoryByProduct("PROD-1");
        Assert.Equal(2, records.Count);
    }
}
