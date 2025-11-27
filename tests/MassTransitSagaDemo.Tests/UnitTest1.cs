using MassTransit;
using MassTransit.Testing;
using MassTransitSagaDemo;
using MassTransit.Messages;
using Microsoft.Extensions.DependencyInjection;

namespace MassTransitSagaDemo.Tests;

public class OrderStateMachineTests
{
    [Fact]
    public async Task Should_Create_Saga_When_Order_Submitted()
    {
        await using var provider = new ServiceCollection()
            .AddMassTransitTestHarness(cfg =>
            {
                cfg.AddSagaStateMachine<OrderStateMachine, OrderState>()
                    .InMemoryRepository();
            })
            .BuildServiceProvider(true);

        var harness = provider.GetRequiredService<ITestHarness>();
        await harness.Start();

        var orderId = Guid.NewGuid();
        var sagaHarness = harness.GetSagaStateMachineHarness<OrderStateMachine, OrderState>();

        await harness.Bus.Publish(new SubmitOrder
        {
            OrderId = orderId,
            Amount = 100.50m,
            ProductId = "PROD-123",
            Quantity = 1
        });

        // Verify saga was created with the correct correlation ID
        Assert.True(await sagaHarness.Created.Any(x => x.CorrelationId == orderId), "Saga should be created with correct OrderId");
        
        // Verify ReserveInventory command was published
        Assert.True(await harness.Published.Any<ReserveInventory>(), "ReserveInventory command should be published");
    }

    [Fact]
    public async Task Should_Transition_To_InventoryReserved_When_Inventory_Reserved()
    {
        await using var provider = new ServiceCollection()
            .AddMassTransitTestHarness(cfg =>
            {
                cfg.AddSagaStateMachine<OrderStateMachine, OrderState>()
                    .InMemoryRepository();
            })
            .BuildServiceProvider(true);

        var harness = provider.GetRequiredService<ITestHarness>();
        await harness.Start();

        var orderId = Guid.NewGuid();
        var sagaHarness = harness.GetSagaStateMachineHarness<OrderStateMachine, OrderState>();

        await harness.Bus.Publish(new SubmitOrder
        {
            OrderId = orderId,
            Amount = 100.50m,
            ProductId = "PROD-123",
            Quantity = 1
        });

        await harness.Bus.Publish(new InventoryReserved
        {
            OrderId = orderId
        });

        // Verify saga was created
        Assert.True(await sagaHarness.Created.Any(x => x.CorrelationId == orderId), "Saga should be created");
        
        // Verify saga received InventoryReserved event
        Assert.True(await sagaHarness.Consumed.Any<InventoryReserved>(), "Saga should consume InventoryReserved");
        
        // Verify ProcessPayment command was published
        Assert.True(await harness.Published.Any<ProcessPayment>(), "ProcessPayment command should be published");
    }

    [Fact]
    public async Task Should_Complete_Order_When_Payment_Successful()
    {
        await using var provider = new ServiceCollection()
            .AddMassTransitTestHarness(cfg =>
            {
                cfg.AddSagaStateMachine<OrderStateMachine, OrderState>()
                    .InMemoryRepository();
            })
            .BuildServiceProvider(true);

        var harness = provider.GetRequiredService<ITestHarness>();
        await harness.Start();

        var orderId = Guid.NewGuid();
        var sagaHarness = harness.GetSagaStateMachineHarness<OrderStateMachine, OrderState>();

        await harness.Bus.Publish(new SubmitOrder
        {
            OrderId = orderId,
            Amount = 100.50m,
            ProductId = "PROD-123",
            Quantity = 1
        });

        await harness.Bus.Publish(new InventoryReserved { OrderId = orderId });
        await harness.Bus.Publish(new PaymentProcessed { OrderId = orderId });

        // Verify saga was created
        Assert.True(await sagaHarness.Created.Any(x => x.CorrelationId == orderId), "Saga should be created");
        
        // Verify all events were consumed
        Assert.True(await sagaHarness.Consumed.Any<SubmitOrder>(), "Saga should consume SubmitOrder");
        Assert.True(await sagaHarness.Consumed.Any<InventoryReserved>(), "Saga should consume InventoryReserved");
        Assert.True(await sagaHarness.Consumed.Any<PaymentProcessed>(), "Saga should consume PaymentProcessed");
        
        // Verify OrderCompleted was published
        Assert.True(await harness.Published.Any<OrderCompleted>(), "OrderCompleted event should be published");
    }

    [Fact]
    public async Task Should_Compensate_When_Payment_Failed()
    {
        await using var provider = new ServiceCollection()
            .AddMassTransitTestHarness(cfg =>
            {
                cfg.AddSagaStateMachine<OrderStateMachine, OrderState>()
                    .InMemoryRepository();
            })
            .BuildServiceProvider(true);

        var harness = provider.GetRequiredService<ITestHarness>();
        await harness.Start();

        var orderId = Guid.NewGuid();
        var sagaHarness = harness.GetSagaStateMachineHarness<OrderStateMachine, OrderState>();

        await harness.Bus.Publish(new SubmitOrder
        {
            OrderId = orderId,
            Amount = 100.50m,
            ProductId = "PROD-123",
            Quantity = 1
        });

        await harness.Bus.Publish(new InventoryReserved { OrderId = orderId });
        await harness.Bus.Publish(new PaymentFailed { OrderId = orderId, Reason = "Insufficient funds" });

        // Verify saga was created
        Assert.True(await sagaHarness.Created.Any(x => x.CorrelationId == orderId), "Saga should be created");
        
        // Verify saga consumed PaymentFailed
        Assert.True(await sagaHarness.Consumed.Any<PaymentFailed>(), "Saga should consume PaymentFailed");
        
        // Verify compensation was triggered
        Assert.True(await harness.Published.Any<ReleaseInventory>(), "ReleaseInventory compensation command should be published");
    }

    [Fact]
    public async Task Should_Publish_ReserveInventory_Command_After_Submit()
    {
        await using var provider = new ServiceCollection()
            .AddMassTransitTestHarness(cfg =>
            {
                cfg.AddSagaStateMachine<OrderStateMachine, OrderState>()
                    .InMemoryRepository();
            })
            .BuildServiceProvider(true);

        var harness = provider.GetRequiredService<ITestHarness>();
        await harness.Start();

        var orderId = Guid.NewGuid();

        await harness.Bus.Publish(new SubmitOrder
        {
            OrderId = orderId,
            Amount = 100.50m,
            ProductId = "PROD-123",
            Quantity = 1
        });

        Assert.True(await harness.Published.Any<ReserveInventory>());
    }

    [Fact]
    public async Task Should_Publish_ProcessPayment_Command_After_Inventory_Reserved()
    {
        await using var provider = new ServiceCollection()
            .AddMassTransitTestHarness(cfg =>
            {
                cfg.AddSagaStateMachine<OrderStateMachine, OrderState>()
                    .InMemoryRepository();
            })
            .BuildServiceProvider(true);

        var harness = provider.GetRequiredService<ITestHarness>();
        await harness.Start();

        var orderId = Guid.NewGuid();

        await harness.Bus.Publish(new SubmitOrder
        {
            OrderId = orderId,
            Amount = 100.50m,
            ProductId = "PROD-123",
            Quantity = 1
        });

        await harness.Bus.Publish(new InventoryReserved { OrderId = orderId });

        Assert.True(await harness.Published.Any<ProcessPayment>());
    }
}
