using System.Collections.Concurrent;

namespace PaymentService;

public class PaymentTrackingService
{
    private readonly ConcurrentDictionary<Guid, PaymentStatus> _payments = new();

    public void AddPayment(Guid orderId, decimal amount, string status, string? reason = null)
    {
        _payments[orderId] = new PaymentStatus
        {
            OrderId = orderId,
            Amount = amount,
            Status = status,
            Reason = reason,
            ProcessedAt = DateTime.UtcNow
        };
    }

    public PaymentStatus? GetPaymentStatus(Guid orderId)
    {
        _payments.TryGetValue(orderId, out var payment);
        return payment;
    }

    public List<PaymentStatus> GetAllPayments()
    {
        return _payments.Values.OrderByDescending(p => p.ProcessedAt).ToList();
    }

    public object GetStatistics()
    {
        var total = _payments.Count;
        var successful = _payments.Values.Count(p => p.Status == "Processed");
        var failed = _payments.Values.Count(p => p.Status == "Failed");
        var totalAmount = _payments.Values.Where(p => p.Status == "Processed").Sum(p => p.Amount);

        return new
        {
            totalPayments = total,
            successfulPayments = successful,
            failedPayments = failed,
            successRate = total > 0 ? Math.Round((double)successful / total * 100, 2) : 0,
            totalAmountProcessed = totalAmount
        };
    }
}

public class PaymentStatus
{
    public Guid OrderId { get; set; }
    public decimal Amount { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Reason { get; set; }
    public DateTime ProcessedAt { get; set; }
}
