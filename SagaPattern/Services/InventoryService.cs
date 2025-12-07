using SagaPattern.Models;

namespace SagaPattern.Services
{
    public class InventoryService
    {
        private readonly Dictionary<string, InventoryItem> _inventory = new();
        private readonly Dictionary<string, InventoryReservation> _reservations = new();
        private readonly object _lock = new();

        public InventoryService()
        {
            // Initialize inventory with sample products
            _inventory["PROD001"] = new InventoryItem
            {
                ProductId = "PROD001",
                ProductName = "Laptop",
                AvailableStock = 10,
                ReservedStock = 0
            };

            _inventory["PROD002"] = new InventoryItem
            {
                ProductId = "PROD002",
                ProductName = "Mouse",
                AvailableStock = 2,  // Low stock for testing failures
                ReservedStock = 0
            };

            _inventory["PROD003"] = new InventoryItem
            {
                ProductId = "PROD003",
                ProductName = "Keyboard",
                AvailableStock = 50,
                ReservedStock = 0
            };
        }

        public InventoryReservation? ReserveStock(string orderId, string productId, int quantity, bool simulateFailure = false)
        {
            lock (_lock)
            {
                Console.WriteLine($"[InventoryService] Attempting to reserve stock");
                Console.WriteLine($"  → Product: {productId}, Quantity: {quantity}");

                if (!_inventory.TryGetValue(productId, out var item))
                {
                    Console.WriteLine($"[InventoryService] Product not found: {productId}");
                    return null;
                }

                // Simulate failure for testing
                if (simulateFailure)
                {
                    Console.WriteLine($"[InventoryService] FAILED: Simulated inventory failure");
                    return null;
                }

                // Check if enough stock is available
                if (item.AvailableStock < quantity)
                {
                    Console.WriteLine($"[InventoryService] FAILED: Insufficient stock");
                    Console.WriteLine($"  → Available: {item.AvailableStock}, Required: {quantity}");
                    return null;
                }

                // Reserve the stock
                item.AvailableStock -= quantity;
                item.ReservedStock += quantity;
                item.LastUpdated = DateTime.UtcNow;

                var reservation = new InventoryReservation
                {
                    OrderId = orderId,
                    ProductId = productId,
                    Quantity = quantity,
                    Status = InventoryStatus.Reserved
                };

                _reservations[reservation.ReservationId] = reservation;

                Console.WriteLine($"[InventoryService] Stock reserved: {reservation.ReservationId}");
                Console.WriteLine($"  → Product: {item.ProductName}");
                Console.WriteLine($"  → Reserved: {quantity}, Remaining: {item.AvailableStock}");

                return reservation;
            }
        }

        public bool ReleaseReservation(string reservationId)
        {
            lock (_lock)
            {
                if (!_reservations.TryGetValue(reservationId, out var reservation))
                {
                    Console.WriteLine($"[InventoryService] Reservation not found: {reservationId}");
                    return false;
                }

                if (reservation.Status != InventoryStatus.Reserved)
                {
                    Console.WriteLine($"[InventoryService] Cannot release reservation with status: {reservation.Status}");
                    return false;
                }

                if (!_inventory.TryGetValue(reservation.ProductId, out var item))
                {
                    Console.WriteLine($"[InventoryService] Product not found: {reservation.ProductId}");
                    return false;
                }

                // Release the reserved stock back to available
                item.AvailableStock += reservation.Quantity;
                item.ReservedStock -= reservation.Quantity;
                item.LastUpdated = DateTime.UtcNow;

                reservation.Status = InventoryStatus.Released;

                Console.WriteLine($"[InventoryService] Reservation released: {reservationId}");
                Console.WriteLine($"  → Product: {item.ProductName}");
                Console.WriteLine($"  → Released: {reservation.Quantity}, Available: {item.AvailableStock}");

                return true;
            }
        }

        public bool ConfirmReservation(string reservationId)
        {
            lock (_lock)
            {
                if (!_reservations.TryGetValue(reservationId, out var reservation))
                {
                    Console.WriteLine($"[InventoryService] Reservation not found: {reservationId}");
                    return false;
                }

                if (!_inventory.TryGetValue(reservation.ProductId, out var item))
                {
                    return false;
                }

                // Move from reserved to sold (remove from reserved stock)
                item.ReservedStock -= reservation.Quantity;
                item.LastUpdated = DateTime.UtcNow;

                Console.WriteLine($"[InventoryService] Reservation confirmed: {reservationId}");

                return true;
            }
        }

        public InventoryItem? GetInventoryItem(string productId)
        {
            lock (_lock)
            {
                return _inventory.TryGetValue(productId, out var item) ? item : null;
            }
        }

        public List<InventoryItem> GetAllInventory()
        {
            lock (_lock)
            {
                return _inventory.Values.ToList();
            }
        }
    }
}
