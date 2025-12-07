using Microsoft.AspNetCore.Mvc;
using Saga.Api.Services;
using Saga.Contracts.DTOs;

namespace Saga.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SagaController : ControllerBase
    {
        private readonly SagaOrchestratorService _orchestrator;
        private readonly ILogger<SagaController> _logger;

        public SagaController(SagaOrchestratorService orchestrator, ILogger<SagaController> logger)
        {
            _orchestrator = orchestrator;
            _logger = logger;
        }

        [HttpPost("execute")]
        public async Task<ActionResult<SagaTransactionResponse>> ExecuteSaga([FromBody] ExecuteSagaRequest request)
        {
            try
            {
                _logger.LogInformation(
                    "Executing saga for Customer: {CustomerId}, Product: {ProductId}",
                    request.CustomerId, request.ProductId);

                var saga = await _orchestrator.ExecuteSagaAsync(
                    request.CustomerId,
                    request.ProductId,
                    request.Quantity,
                    request.Amount);

                var response = new SagaTransactionResponse
                {
                    TransactionId = saga.TransactionId,
                    OrderId = saga.OrderId,
                    PaymentId = saga.PaymentId,
                    State = saga.State.ToString(),
                    StartedAt = saga.StartedAt,
                    CompletedAt = saga.CompletedAt,
                    ExecutedSteps = saga.ExecutedSteps,
                    CompensatedSteps = saga.CompensatedSteps,
                    ErrorMessage = saga.ErrorMessage,
                    DurationSeconds = saga.CompletedAt.HasValue
                        ? (saga.CompletedAt.Value - saga.StartedAt).TotalSeconds
                        : null
                };

                if (saga.State == Models.SagaState.Completed)
                {
                    return Ok(response);
                }
                else
                {
                    return BadRequest(response);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing saga");
                return StatusCode(500, new { error = "Failed to execute saga transaction" });
            }
        }

        [HttpGet("{id}")]
        public ActionResult<SagaTransactionResponse> GetTransaction(string id)
        {
            var saga = _orchestrator.GetTransaction(id);
            if (saga == null)
            {
                return NotFound(new { error = $"Transaction {id} not found" });
            }

            var response = new SagaTransactionResponse
            {
                TransactionId = saga.TransactionId,
                OrderId = saga.OrderId,
                PaymentId = saga.PaymentId,
                State = saga.State.ToString(),
                StartedAt = saga.StartedAt,
                CompletedAt = saga.CompletedAt,
                ExecutedSteps = saga.ExecutedSteps,
                CompensatedSteps = saga.CompensatedSteps,
                ErrorMessage = saga.ErrorMessage,
                DurationSeconds = saga.CompletedAt.HasValue
                    ? (saga.CompletedAt.Value - saga.StartedAt).TotalSeconds
                    : null
            };

            return Ok(response);
        }

        [HttpGet]
        public ActionResult<List<SagaTransactionResponse>> GetAllTransactions()
        {
            var sagas = _orchestrator.GetAllTransactions();

            var response = sagas.Select(saga => new SagaTransactionResponse
            {
                TransactionId = saga.TransactionId,
                OrderId = saga.OrderId,
                PaymentId = saga.PaymentId,
                State = saga.State.ToString(),
                StartedAt = saga.StartedAt,
                CompletedAt = saga.CompletedAt,
                ExecutedSteps = saga.ExecutedSteps,
                CompensatedSteps = saga.CompensatedSteps,
                ErrorMessage = saga.ErrorMessage,
                DurationSeconds = saga.CompletedAt.HasValue
                    ? (saga.CompletedAt.Value - saga.StartedAt).TotalSeconds
                    : null
            }).ToList();

            return Ok(response);
        }
    }
}
