using Microsoft.AspNetCore.Mvc;
using OrderService.Api.Services;
using Saga.Contracts.DTOs;

namespace OrderService.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly OrderManagementService _orderService;
        private readonly ILogger<OrdersController> _logger;

        public OrdersController(OrderManagementService orderService, ILogger<OrdersController> logger)
        {
            _orderService = orderService;
            _logger = logger;
        }

        [HttpPost]
        public ActionResult<OrderResponse> CreateOrder([FromBody] CreateOrderRequest request)
        {
            try
            {
                _logger.LogInformation("Creating order for customer {CustomerId}", request.CustomerId);

                var order = _orderService.CreateOrder(
                    request.CustomerId,
                    request.ProductId,
                    request.Quantity,
                    request.Amount);

                var response = new OrderResponse
                {
                    OrderId = order.OrderId,
                    CustomerId = order.CustomerId,
                    ProductId = order.ProductId,
                    Quantity = order.Quantity,
                    Amount = order.Amount,
                    Status = order.Status.ToString(),
                    CreatedAt = order.CreatedAt,
                    UpdatedAt = order.UpdatedAt
                };

                return CreatedAtAction(nameof(GetOrder), new { id = order.OrderId }, response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating order");
                return StatusCode(500, new { error = "Failed to create order" });
            }
        }

        [HttpGet("{id}")]
        public ActionResult<OrderResponse> GetOrder(string id)
        {
            var order = _orderService.GetOrder(id);
            if (order == null)
            {
                return NotFound(new { error = $"Order {id} not found" });
            }

            var response = new OrderResponse
            {
                OrderId = order.OrderId,
                CustomerId = order.CustomerId,
                ProductId = order.ProductId,
                Quantity = order.Quantity,
                Amount = order.Amount,
                Status = order.Status.ToString(),
                CreatedAt = order.CreatedAt,
                UpdatedAt = order.UpdatedAt
            };

            return Ok(response);
        }

        [HttpGet]
        public ActionResult<List<OrderResponse>> GetAllOrders([FromQuery] string? customerId = null)
        {
            var orders = string.IsNullOrEmpty(customerId)
                ? _orderService.GetAllOrders()
                : _orderService.GetOrdersByCustomer(customerId);

            var response = orders.Select(o => new OrderResponse
            {
                OrderId = o.OrderId,
                CustomerId = o.CustomerId,
                ProductId = o.ProductId,
                Quantity = o.Quantity,
                Amount = o.Amount,
                Status = o.Status.ToString(),
                CreatedAt = o.CreatedAt,
                UpdatedAt = o.UpdatedAt
            }).ToList();

            return Ok(response);
        }

        [HttpPut("{id}/confirm")]
        public ActionResult ConfirmOrder(string id)
        {
            var success = _orderService.ConfirmOrder(id);
            if (!success)
            {
                return NotFound(new { error = $"Order {id} not found" });
            }

            return Ok(new { message = "Order confirmed successfully" });
        }

        [HttpPut("{id}/cancel")]
        public ActionResult CancelOrder(string id, [FromBody] OrderStatusUpdateRequest request)
        {
            var success = _orderService.CancelOrder(id, request.Reason ?? "Cancelled by request");
            if (!success)
            {
                return NotFound(new { error = $"Order {id} not found" });
            }

            return Ok(new { message = "Order cancelled successfully" });
        }

        [HttpPut("{id}/fail")]
        public ActionResult FailOrder(string id, [FromBody] OrderStatusUpdateRequest request)
        {
            var success = _orderService.MarkOrderFailed(id, request.Reason ?? "Failed");
            if (!success)
            {
                return NotFound(new { error = $"Order {id} not found" });
            }

            return Ok(new { message = "Order marked as failed" });
        }
    }
}
