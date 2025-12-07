using Microsoft.AspNetCore.Mvc;

namespace InventoryService;

[ApiController]
[Route("api/[controller]")]
public class InventoryController : ControllerBase
{
    private readonly ILogger<InventoryController> _logger;
    private readonly InventoryTrackingService _inventoryTracking;

    public InventoryController(ILogger<InventoryController> logger, InventoryTrackingService inventoryTracking)
    {
        _logger = logger;
        _inventoryTracking = inventoryTracking;
    }

    [HttpGet("order/{orderId}")]
    public IActionResult GetInventoryByOrder(Guid orderId)
    {
        var inventory = _inventoryTracking.GetInventoryByOrder(orderId);
        if (inventory == null)
        {
            return NotFound(new { message = "Inventory record not found" });
        }

        return Ok(inventory);
    }

    [HttpGet]
    public IActionResult GetAllInventoryRecords()
    {
        var records = _inventoryTracking.GetAllInventoryRecords();
        return Ok(records);
    }

    [HttpGet("product/{productId}")]
    public IActionResult GetProductInventory(string productId)
    {
        var records = _inventoryTracking.GetInventoryByProduct(productId);
        return Ok(records);
    }

    [HttpGet("stats")]
    public IActionResult GetStatistics()
    {
        var stats = _inventoryTracking.GetStatistics();
        return Ok(stats);
    }
}
