using Microsoft.AspNetCore.Mvc;
using PaymentService.Api.Services;
using Saga.Contracts.DTOs;

namespace PaymentService.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentsController : ControllerBase
    {
        private readonly PaymentProcessingService _paymentService;
        private readonly ILogger<PaymentsController> _logger;

        public PaymentsController(PaymentProcessingService paymentService, ILogger<PaymentsController> logger)
        {
            _paymentService = paymentService;
            _logger = logger;
        }

        [HttpPost]
        public ActionResult<PaymentResponse> ProcessPayment([FromBody] ProcessPaymentRequest request)
        {
            try
            {
                _logger.LogInformation(
                    "Received payment request for Order: {OrderId}, Customer: {CustomerId}",
                    request.OrderId, request.CustomerId);

                var payment = _paymentService.ProcessPayment(
                    request.OrderId,
                    request.CustomerId,
                    request.Amount);

                var response = new PaymentResponse
                {
                    PaymentId = payment.PaymentId,
                    OrderId = payment.OrderId,
                    CustomerId = payment.CustomerId,
                    Amount = payment.Amount,
                    Status = payment.Status.ToString(),
                    ProcessedAt = payment.ProcessedAt,
                    FailureReason = payment.FailureReason
                };

                if (payment.Status == Models.PaymentStatus.Failed)
                {
                    return BadRequest(response);
                }

                return CreatedAtAction(nameof(GetPayment), new { id = payment.PaymentId }, response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing payment");
                return StatusCode(500, new { error = "Failed to process payment" });
            }
        }

        [HttpGet("{id}")]
        public ActionResult<PaymentResponse> GetPayment(string id)
        {
            var payment = _paymentService.GetPayment(id);
            if (payment == null)
            {
                return NotFound(new { error = $"Payment {id} not found" });
            }

            var response = new PaymentResponse
            {
                PaymentId = payment.PaymentId,
                OrderId = payment.OrderId,
                CustomerId = payment.CustomerId,
                Amount = payment.Amount,
                Status = payment.Status.ToString(),
                ProcessedAt = payment.ProcessedAt,
                FailureReason = payment.FailureReason
            };

            return Ok(response);
        }

        [HttpGet]
        public ActionResult<List<PaymentResponse>> GetAllPayments([FromQuery] string? customerId = null)
        {
            var payments = string.IsNullOrEmpty(customerId)
                ? _paymentService.GetAllPayments()
                : _paymentService.GetPaymentsByCustomer(customerId);

            var response = payments.Select(p => new PaymentResponse
            {
                PaymentId = p.PaymentId,
                OrderId = p.OrderId,
                CustomerId = p.CustomerId,
                Amount = p.Amount,
                Status = p.Status.ToString(),
                ProcessedAt = p.ProcessedAt,
                FailureReason = p.FailureReason
            }).ToList();

            return Ok(response);
        }

        [HttpPost("{id}/refund")]
        public ActionResult RefundPayment(string id)
        {
            var success = _paymentService.RefundPayment(id);
            if (!success)
            {
                return BadRequest(new { error = $"Cannot refund payment {id}" });
            }

            return Ok(new { message = "Payment refunded successfully" });
        }

        [HttpGet("customers/{customerId}/balance")]
        public ActionResult<CustomerBalanceResponse> GetCustomerBalance(string customerId)
        {
            var balance = _paymentService.GetCustomerBalance(customerId);

            var response = new CustomerBalanceResponse
            {
                CustomerId = customerId,
                Balance = balance
            };

            return Ok(response);
        }
    }
}
