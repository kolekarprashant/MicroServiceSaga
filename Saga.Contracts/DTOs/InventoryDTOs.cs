namespace Saga.Contracts.DTOs
{
    public class ReserveInventoryRequest
    {
        public string OrderId { get; set; } = string.Empty;
        public string ProductId { get; set; } = string.Empty;
        public int Quantity { get; set; }
    }

    public class InventoryReservationResponse
    {
        public string ReservationId { get; set; } = string.Empty;
        public string OrderId { get; set; } = string.Empty;
        public string ProductId { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime ReservedAt { get; set; }
    }

    public class ReleaseInventoryRequest
    {
        public string ReservationId { get; set; } = string.Empty;
    }

    public class InventoryItemResponse
    {
        public string ProductId { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public int AvailableStock { get; set; }
        public int ReservedStock { get; set; }
    }
}
