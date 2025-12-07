using MassTransit;
using MassTransit.Messages;

namespace PaymentService;

public class ProcessPaymentConsumer : IConsumer<ProcessPayment>
{
    private readonly ILogger<ProcessPaymentConsumer> _logger;
    private readonly PaymentTrackingService _paymentTracking;

    public ProcessPaymentConsumer(ILogger<ProcessPaymentConsumer> logger, PaymentTrackingService paymentTracking)
    {
        _logger = logger;
        _paymentTracking = paymentTracking;
    }

    public async Task Consume(ConsumeContext<ProcessPayment> context)
    {
        _logger.LogInformation("💳 PaymentService: Processing payment for Order {OrderId}, Amount: ${Amount}", 
            context.Message.OrderId, context.Message.Amount);

        // Simulate payment processing time
        await Task.Delay(500);

        // Simulate payment validation logic
        // For demo: payments over $300 have a higher chance of failure
        var random = new Random(context.Message.OrderId.GetHashCode());
        var failureChance = context.Message.Amount > 300 ? 0.4 : 0.2;
        bool paymentFailed = random.NextDouble() < failureChance;

        if (paymentFailed)
        {
            var reasons = new[]
            {
                "Insufficient funds",
                "Card declined",
                "Payment gateway timeout",
                "Invalid card details",
                "Fraud detection triggered"
            };
            var reason = reasons[random.Next(reasons.Length)];

            _logger.LogWarning("💳 PaymentService: Payment FAILED for Order {OrderId}. Reason: {Reason}", 
                context.Message.OrderId, reason);

            _paymentTracking.AddPayment(context.Message.OrderId, context.Message.Amount, "Failed", reason);

            await context.Publish(new PaymentFailed 
            { 
                OrderId = context.Message.OrderId, 
                Reason = reason 
            });
        }
        else
        {
            _logger.LogInformation("💳 PaymentService: Payment SUCCESSFUL for Order {OrderId}", 
                context.Message.OrderId);

            _paymentTracking.AddPayment(context.Message.OrderId, context.Message.Amount, "Processed");

            await context.Publish(new PaymentProcessed 
            { 
                OrderId = context.Message.OrderId 
            });
        }
    }
}
