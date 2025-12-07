namespace SagaPattern.Models
{
    public class InventoryItem
    {
        public string ProductId { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public int AvailableStock { get; set; }
        public int ReservedStock { get; set; }
        public DateTime LastUpdated { get; set; }

        public InventoryItem()
        {
            LastUpdated = DateTime.UtcNow;
        }
    }

    public class InventoryReservation
    {
        public string ReservationId { get; set; } = string.Empty;
        public string OrderId { get; set; } = string.Empty;
        public string ProductId { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public InventoryStatus Status { get; set; }
        public DateTime ReservedAt { get; set; }

        public InventoryReservation()
        {
            ReservationId = Guid.NewGuid().ToString();
            ReservedAt = DateTime.UtcNow;
            Status = InventoryStatus.Reserved;
        }
    }
}
