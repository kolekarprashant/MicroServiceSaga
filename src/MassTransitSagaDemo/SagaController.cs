using Microsoft.AspNetCore.Mvc;

namespace MassTransitSagaDemo;

[ApiController]
[Route("api/[controller]")]
public class SagaController : ControllerBase
{
    private readonly ILogger<SagaController> _logger;

    public SagaController(ILogger<SagaController> logger)
    {
        _logger = logger;
    }

    [HttpGet("health")]
    public IActionResult HealthCheck()
    {
        return Ok(new
        {
            service = "Saga Orchestrator",
            status = "Healthy",
            timestamp = DateTime.UtcNow,
            description = "Saga orchestrator is running and coordinating order workflows"
        });
    }

    [HttpGet("info")]
    public IActionResult GetInfo()
    {
        return Ok(new
        {
            service = "Saga Orchestrator",
            version = "1.0.0",
            states = new[] { "Initial", "Submitted", "InventoryReserved", "Completed", "Failed" },
            description = "Orchestrates order workflow: Inventory → Payment → Completion or Compensation"
        });
    }
}
