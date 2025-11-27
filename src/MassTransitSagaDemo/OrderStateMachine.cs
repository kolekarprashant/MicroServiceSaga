using MassTransit;
using MassTransit.Messages;

namespace MassTransitSagaDemo;

public class OrderStateMachine : MassTransitStateMachine<OrderState>
{
    public State Submitted { get; private set; } = null!;
    public State InventoryReservedState { get; private set; } = null!;
    public State PaymentSucceeded { get; private set; } = null!;
    public State Completed { get; private set; } = null!;
    public State Failed { get; private set; } = null!;

    public Event<SubmitOrder> SubmitOrderEvent { get; private set; } = null!;
    public Event<InventoryReserved> InventoryReservedEvent { get; private set; } = null!;
    public Event<PaymentProcessed> PaymentProcessedEvent { get; private set; } = null!;
    public Event<PaymentFailed> PaymentFailedEvent { get; private set; } = null!;
    public Event<InventoryReleased> InventoryReleasedEvent { get; private set; } = null!;

    public OrderStateMachine()
    {
        InstanceState(x => x.CurrentState);

        Event(() => SubmitOrderEvent, x => x.CorrelateById(m => m.Message.OrderId));
        Event(() => InventoryReservedEvent, x => x.CorrelateById(m => m.Message.OrderId));
        Event(() => PaymentProcessedEvent, x => x.CorrelateById(m => m.Message.OrderId));
        Event(() => PaymentFailedEvent, x => x.CorrelateById(m => m.Message.OrderId));
        Event(() => InventoryReleasedEvent, x => x.CorrelateById(m => m.Message.OrderId));

        Initially(
            When(SubmitOrderEvent)
                .Then(context =>
                {
                    context.Saga.Created = DateTime.UtcNow;
                    context.Saga.Amount = context.Message.Amount;
                    Console.WriteLine($"\n═══════════════════════════════════════════════════════════");
                    Console.WriteLine($"🎯 [SAGA EVENT] SubmitOrder received");
                    Console.WriteLine($"   OrderId: {context.Message.OrderId}");
                    Console.WriteLine($"   Amount: ${context.Message.Amount}");
                    Console.WriteLine($"   ProductId: {context.Message.ProductId}");
                    Console.WriteLine($"   Quantity: {context.Message.Quantity}");
                    Console.WriteLine($"   State: Initial → Submitted");
                    Console.WriteLine($"═══════════════════════════════════════════════════════════\n");
                })
                .ThenAsync(context =>
                {
                    Console.WriteLine($"🎯 [SAGA COMMAND] Publishing ReserveInventory for {context.Message.OrderId}");
                    return context.Publish(new ReserveInventory 
                    { 
                        OrderId = context.Message.OrderId,
                        ProductId = context.Message.ProductId,
                        Quantity = context.Message.Quantity
                    });
                })
                .TransitionTo(Submitted)
        );

        During(Submitted,
            When(InventoryReservedEvent)
                .Then(context =>
                {
                    context.Saga.InventoryReserved = true;
                    Console.WriteLine($"\n═══════════════════════════════════════════════════════════");
                    Console.WriteLine($"🎯 [SAGA EVENT] InventoryReserved received");
                    Console.WriteLine($"   OrderId: {context.Message.OrderId}");
                    Console.WriteLine($"   State: Submitted → InventoryReserved");
                    Console.WriteLine($"═══════════════════════════════════════════════════════════\n");
                })
                .ThenAsync(context =>
                {
                    Console.WriteLine($"🎯 [SAGA COMMAND] Publishing ProcessPayment for {context.Message.OrderId} (Amount: ${context.Saga.Amount})");
                    return context.Publish(new ProcessPayment 
                    { 
                        OrderId = context.Message.OrderId, 
                        Amount = context.Saga.Amount 
                    });
                })
                .TransitionTo(InventoryReservedState)
        );

        During(InventoryReservedState,
            When(PaymentProcessedEvent)
                .Then(context =>
                {
                    context.Saga.PaymentProcessed = true;
                    Console.WriteLine($"\n═══════════════════════════════════════════════════════════");
                    Console.WriteLine($"🎯 [SAGA EVENT] PaymentProcessed received");
                    Console.WriteLine($"   OrderId: {context.Message.OrderId}");
                    Console.WriteLine($"   State: InventoryReserved → Completed");
                    Console.WriteLine($"   ✅ ORDER SUCCESSFUL!");
                    Console.WriteLine($"═══════════════════════════════════════════════════════════\n");
                })
                .ThenAsync(context =>
                {
                    Console.WriteLine($"🎯 [SAGA COMMAND] Publishing OrderCompleted for {context.Message.OrderId}");
                    return context.Publish(new OrderCompleted 
                    { 
                        OrderId = context.Message.OrderId,
                        Amount = context.Saga.Amount
                    });
                })
                .TransitionTo(Completed)
                .Finalize(),

            When(PaymentFailedEvent)
                .Then(context =>
                {
                    Console.WriteLine($"\n═══════════════════════════════════════════════════════════");
                    Console.WriteLine($"🎯 [SAGA EVENT] PaymentFailed received");
                    Console.WriteLine($"   OrderId: {context.Message.OrderId}");
                    Console.WriteLine($"   Reason: {context.Message.Reason}");
                    Console.WriteLine($"   State: InventoryReserved → Failed");
                    Console.WriteLine($"   ⚠️ INITIATING COMPENSATION");
                    Console.WriteLine($"═══════════════════════════════════════════════════════════\n");
                })
                // Compensating action: release inventory
                .ThenAsync(context =>
                {
                    Console.WriteLine($"🎯 [SAGA COMPENSATION] Publishing ReleaseInventory for {context.Message.OrderId}");
                    return context.Publish(new ReleaseInventory { OrderId = context.Message.OrderId });
                })
                .TransitionTo(Failed)
        );

        During(Failed,
            When(InventoryReleasedEvent)
                .Then(context =>
                {
                    context.Saga.InventoryReleased = true;
                    Console.WriteLine($"\n═══════════════════════════════════════════════════════════");
                    Console.WriteLine($"🎯 [SAGA EVENT] InventoryReleased received (Compensation)");
                    Console.WriteLine($"   OrderId: {context.Message.OrderId}");
                    Console.WriteLine($"   ✅ COMPENSATION COMPLETE");
                    Console.WriteLine($"═══════════════════════════════════════════════════════════\n");
                })
                .ThenAsync(context =>
                {
                    Console.WriteLine($"🎯 [SAGA COMMAND] Publishing OrderFailed for {context.Message.OrderId}");
                    return context.Publish(new OrderFailed 
                    { 
                        OrderId = context.Message.OrderId, 
                        Reason = "Payment failed - order cancelled" 
                    });
                })
                .Finalize()
        );

        SetCompletedWhenFinalized();
    }
}
