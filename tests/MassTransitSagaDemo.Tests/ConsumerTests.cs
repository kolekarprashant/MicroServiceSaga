using MassTransit;
using MassTransit.Testing;
using MassTransit.Messages;
using PaymentService;
using InventoryService;
using Microsoft.Extensions.DependencyInjection;

namespace MassTransitSagaDemo.Tests;

public class ConsumerTests
{
    [Fact]
    public async Task ProcessPaymentConsumer_Should_Consume_Message()
    {
        await using var provider = new ServiceCollection()
            .AddMassTransitTestHarness(cfg =>
            {
                cfg.AddConsumer<ProcessPaymentConsumer>();
            })
            .AddSingleton<PaymentTrackingService>()
            .BuildServiceProvider(true);

        var harness = provider.GetRequiredService<ITestHarness>();
        await harness.Start();

        var orderId = Guid.NewGuid();
        await harness.Bus.Publish(new ProcessPayment
        {
            OrderId = orderId,
            Amount = 100.50m
        });

        Assert.True(await harness.Consumed.Any<ProcessPayment>());
    }

    [Fact]
    public async Task ReserveInventoryConsumer_Should_Consume_Message()
    {
        await using var provider = new ServiceCollection()
            .AddMassTransitTestHarness(cfg =>
            {
                cfg.AddConsumer<ReserveInventoryConsumer>();
            })
            .AddSingleton<InventoryTrackingService>()
            .BuildServiceProvider(true);

        var harness = provider.GetRequiredService<ITestHarness>();
        await harness.Start();

        var orderId = Guid.NewGuid();
        await harness.Bus.Publish(new ReserveInventory
        {
            OrderId = orderId
        });

        Assert.True(await harness.Consumed.Any<ReserveInventory>());
    }

    [Fact]
    public async Task ReleaseInventoryConsumer_Should_Consume_Message()
    {
        await using var provider = new ServiceCollection()
            .AddMassTransitTestHarness(cfg =>
            {
                cfg.AddConsumer<ReleaseInventoryConsumer>();
            })
            .AddSingleton<InventoryTrackingService>()
            .BuildServiceProvider(true);

        var harness = provider.GetRequiredService<ITestHarness>();
        await harness.Start();

        var orderId = Guid.NewGuid();
        await harness.Bus.Publish(new ReleaseInventory
        {
            OrderId = orderId
        });

        Assert.True(await harness.Consumed.Any<ReleaseInventory>());
    }

    [Fact]
    public async Task ProcessPaymentConsumer_Should_Publish_PaymentProcessed_On_Success()
    {
        await using var provider = new ServiceCollection()
            .AddMassTransitTestHarness(cfg =>
            {
                cfg.AddConsumer<ProcessPaymentConsumer>();
            })
            .AddSingleton<PaymentTrackingService>()
            .BuildServiceProvider(true);

        var harness = provider.GetRequiredService<ITestHarness>();
        await harness.Start();

        // Try multiple times since payment has random success/failure
        bool published = false;
        for (int i = 0; i < 20; i++)
        {
            await harness.Bus.Publish(new ProcessPayment
            {
                OrderId = Guid.NewGuid(),
                Amount = 100.50m
            });

            // Wait for payment processing (which has a 500ms delay)
            await Task.Delay(600);

            if (await harness.Published.Any<PaymentProcessed>())
            {
                published = true;
                break;
            }
        }

        Assert.True(published, "PaymentProcessed should be published after successful payment");
    }

    [Fact]
    public async Task ReserveInventoryConsumer_Should_Publish_InventoryReserved()
    {
        await using var provider = new ServiceCollection()
            .AddMassTransitTestHarness(cfg =>
            {
                cfg.AddConsumer<ReserveInventoryConsumer>();
            })
            .AddSingleton<InventoryTrackingService>()
            .BuildServiceProvider(true);

        var harness = provider.GetRequiredService<ITestHarness>();
        await harness.Start();

        await harness.Bus.Publish(new ReserveInventory
        {
            OrderId = Guid.NewGuid()
        });

        Assert.True(await harness.Published.Any<InventoryReserved>());
    }
}
