using Microsoft.AspNetCore.Mvc;

namespace PaymentService;

[ApiController]
[Route("api/[controller]")]
public class PaymentsController : ControllerBase
{
    private readonly ILogger<PaymentsController> _logger;
    private readonly PaymentTrackingService _paymentTracking;

    public PaymentsController(ILogger<PaymentsController> logger, PaymentTrackingService paymentTracking)
    {
        _logger = logger;
        _paymentTracking = paymentTracking;
    }

    [HttpGet("{orderId}")]
    public IActionResult GetPaymentStatus(Guid orderId)
    {
        var status = _paymentTracking.GetPaymentStatus(orderId);
        if (status == null)
        {
            return NotFound(new { message = "Payment not found" });
        }

        return Ok(status);
    }

    [HttpGet]
    public IActionResult GetAllPayments()
    {
        var payments = _paymentTracking.GetAllPayments();
        return Ok(payments);
    }

    [HttpGet("stats")]
    public IActionResult GetStatistics()
    {
        var stats = _paymentTracking.GetStatistics();
        return Ok(stats);
    }
}
