using PaymentService;

namespace MassTransitSagaDemo.Tests;

public class PaymentTrackingServiceTests
{
    [Fact]
    public void Should_Add_Payment_Successfully()
    {
        var service = new PaymentTrackingService();
        var orderId = Guid.NewGuid();

        service.AddPayment(orderId, 100.50m, "Processed");
        var payment = service.GetPaymentStatus(orderId);

        Assert.NotNull(payment);
        Assert.Equal(orderId, payment.OrderId);
        Assert.Equal(100.50m, payment.Amount);
        Assert.Equal("Processed", payment.Status);
    }

    [Fact]
    public void Should_Track_Failed_Payment()
    {
        var service = new PaymentTrackingService();
        var orderId = Guid.NewGuid();

        service.AddPayment(orderId, 100.50m, "Failed", "Insufficient funds");
        var payment = service.GetPaymentStatus(orderId);

        Assert.NotNull(payment);
        Assert.Equal("Failed", payment.Status);
        Assert.Equal("Insufficient funds", payment.Reason);
    }

    [Fact]
    public void Should_Return_Null_For_NonExistent_Payment()
    {
        var service = new PaymentTrackingService();
        var payment = service.GetPaymentStatus(Guid.NewGuid());

        Assert.Null(payment);
    }

    [Fact]
    public void Should_Calculate_Statistics_Correctly()
    {
        var service = new PaymentTrackingService();

        service.AddPayment(Guid.NewGuid(), 100m, "Processed");
        service.AddPayment(Guid.NewGuid(), 200m, "Processed");
        service.AddPayment(Guid.NewGuid(), 150m, "Failed");
        service.AddPayment(Guid.NewGuid(), 300m, "Processed");

        var stats = service.GetStatistics();
        var type = stats.GetType();
        
        Assert.Equal(4, type.GetProperty("totalPayments")?.GetValue(stats));
        Assert.Equal(3, type.GetProperty("successfulPayments")?.GetValue(stats));
        Assert.Equal(1, type.GetProperty("failedPayments")?.GetValue(stats));
        Assert.Equal(75.0, type.GetProperty("successRate")?.GetValue(stats)); // 3 out of 4 = 75%
    }

    [Fact]
    public void Should_Handle_Zero_Payments_Statistics()
    {
        var service = new PaymentTrackingService();
        var stats = service.GetStatistics();
        var type = stats.GetType();

        Assert.Equal(0, type.GetProperty("totalPayments")?.GetValue(stats));
        Assert.Equal(0, type.GetProperty("successfulPayments")?.GetValue(stats));
        Assert.Equal(0, type.GetProperty("failedPayments")?.GetValue(stats));
        Assert.Equal(0.0, type.GetProperty("successRate")?.GetValue(stats));
    }

    [Fact]
    public void Should_Get_All_Payments()
    {
        var service = new PaymentTrackingService();
        
        service.AddPayment(Guid.NewGuid(), 100m, "Processed");
        service.AddPayment(Guid.NewGuid(), 200m, "Failed");
        service.AddPayment(Guid.NewGuid(), 300m, "Processed");

        var payments = service.GetAllPayments();
        Assert.Equal(3, payments.Count);
    }
}
