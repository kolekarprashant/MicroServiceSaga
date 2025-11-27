using System.Collections.Concurrent;

namespace InventoryService;

public class InventoryTrackingService
{
    private readonly ConcurrentDictionary<Guid, InventoryRecord> _inventory = new();

    public void AddReservation(Guid orderId, string productId, int quantity)
    {
        _inventory[orderId] = new InventoryRecord
        {
            OrderId = orderId,
            ProductId = productId,
            Quantity = quantity,
            Status = "Reserved",
            ReservedAt = DateTime.UtcNow
        };
    }

    public void ReleaseReservation(Guid orderId)
    {
        if (_inventory.TryGetValue(orderId, out var record))
        {
            record.Status = "Released";
            record.ReleasedAt = DateTime.UtcNow;
        }
    }

    public InventoryRecord? GetInventoryByOrder(Guid orderId)
    {
        _inventory.TryGetValue(orderId, out var record);
        return record;
    }

    public List<InventoryRecord> GetAllInventoryRecords()
    {
        return _inventory.Values.OrderByDescending(i => i.ReservedAt).ToList();
    }

    public List<InventoryRecord> GetInventoryByProduct(string productId)
    {
        return _inventory.Values
            .Where(i => i.ProductId == productId)
            .OrderByDescending(i => i.ReservedAt)
            .ToList();
    }

    public object GetStatistics()
    {
        var total = _inventory.Count;
        var reserved = _inventory.Values.Count(i => i.Status == "Reserved");
        var released = _inventory.Values.Count(i => i.Status == "Released");

        return new
        {
            totalRecords = total,
            currentlyReserved = reserved,
            released = released,
            products = _inventory.Values
                .GroupBy(i => i.ProductId)
                .Select(g => new { productId = g.Key, count = g.Count() })
                .ToList()
        };
    }
}

public class InventoryRecord
{
    public Guid OrderId { get; set; }
    public string ProductId { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime ReservedAt { get; set; }
    public DateTime? ReleasedAt { get; set; }
}
