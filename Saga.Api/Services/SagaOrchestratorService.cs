using Saga.Api.Models;
using Saga.Contracts.DTOs;
using System.Text;
using System.Text.Json;

namespace Saga.Api.Services
{
    public class SagaOrchestratorService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<SagaOrchestratorService> _logger;
        private readonly Dictionary<string, SagaTransaction> _transactions = new();
        private readonly object _lock = new();

        private const string OrderServiceUrl = "http://localhost:5001";
        private const string PaymentServiceUrl = "http://localhost:5002";

        public SagaOrchestratorService(
            IHttpClientFactory httpClientFactory,
            ILogger<SagaOrchestratorService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public async Task<SagaTransaction> ExecuteSagaAsync(
            string customerId,
            string productId,
            int quantity,
            decimal amount)
        {
            var saga = new SagaTransaction();
            lock (_lock)
            {
                _transactions[saga.TransactionId] = saga;
            }

            _logger.LogInformation(
                "===== SAGA TRANSACTION STARTED: {TransactionId} =====",
                saga.TransactionId);

            try
            {
                // Step 1: Create Order
                _logger.LogInformation("--- STEP 1: CREATE ORDER ---");
                var orderResponse = await CreateOrderAsync(customerId, productId, quantity, amount);
                
                saga.OrderId = orderResponse.OrderId;
                saga.State = SagaState.OrderCreated;
                saga.ExecutedSteps.Add("OrderCreated");

                // Step 2: Process Payment
                _logger.LogInformation("--- STEP 2: PROCESS PAYMENT ---");
                var paymentResponse = await ProcessPaymentAsync(orderResponse.OrderId, customerId, amount);
                
                saga.PaymentId = paymentResponse.PaymentId;

                if (paymentResponse.Status != "Success")
                {
                    saga.ErrorMessage = $"Payment failed: {paymentResponse.FailureReason}";
                    saga.State = SagaState.Failed;
                    await CompensateSagaAsync(saga);
                    return saga;
                }

                saga.State = SagaState.PaymentProcessed;
                saga.ExecutedSteps.Add("PaymentProcessed");

                // Step 3: Confirm Order
                _logger.LogInformation("--- STEP 3: CONFIRM ORDER ---");
                await ConfirmOrderAsync(orderResponse.OrderId);

                saga.State = SagaState.Completed;
                saga.CompletedAt = DateTime.UtcNow;

                _logger.LogInformation(
                    "===== SAGA TRANSACTION COMPLETED: {TransactionId} =====",
                    saga.TransactionId);

                return saga;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Saga execution failed");
                saga.ErrorMessage = ex.Message;
                saga.State = SagaState.Failed;
                await CompensateSagaAsync(saga);
                return saga;
            }
        }

        private async Task CompensateSagaAsync(SagaTransaction saga)
        {
            _logger.LogWarning(
                "!!!!! SAGA TRANSACTION FAILED - STARTING COMPENSATION: {TransactionId} !!!!!",
                saga.TransactionId);
            _logger.LogWarning("Error: {ErrorMessage}", saga.ErrorMessage);

            saga.State = SagaState.Compensating;

            var stepsToCompensate = new List<string>(saga.ExecutedSteps);
            stepsToCompensate.Reverse();

            foreach (var step in stepsToCompensate)
            {
                _logger.LogInformation("--- COMPENSATING: {Step} ---", step);

                try
                {
                    switch (step)
                    {
                        case "PaymentProcessed":
                            if (!string.IsNullOrEmpty(saga.PaymentId))
                            {
                                await RefundPaymentAsync(saga.PaymentId);
                                saga.CompensatedSteps.Add("PaymentRefunded");
                            }
                            break;

                        case "OrderCreated":
                            if (!string.IsNullOrEmpty(saga.OrderId))
                            {
                                await CancelOrderAsync(saga.OrderId, saga.ErrorMessage ?? "Saga failed");
                                saga.CompensatedSteps.Add("OrderCancelled");
                            }
                            break;
                    }

                    await Task.Delay(200); // Simulate compensation delay
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Compensation step failed: {Step}", step);
                }
            }

            saga.State = SagaState.Compensated;
            saga.CompletedAt = DateTime.UtcNow;

            _logger.LogWarning(
                "!!!!! SAGA COMPENSATION COMPLETED: {TransactionId} !!!!!",
                saga.TransactionId);
        }

        private async Task<OrderResponse> CreateOrderAsync(
            string customerId, string productId, int quantity, decimal amount)
        {
            var client = _httpClientFactory.CreateClient();
            var request = new CreateOrderRequest
            {
                CustomerId = customerId,
                ProductId = productId,
                Quantity = quantity,
                Amount = amount
            };

            var content = new StringContent(
                JsonSerializer.Serialize(request),
                Encoding.UTF8,
                "application/json");

            var response = await client.PostAsync($"{OrderServiceUrl}/api/orders", content);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<OrderResponse>(responseContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            })!;
        }

        private async Task<PaymentResponse> ProcessPaymentAsync(
            string orderId, string customerId, decimal amount)
        {
            var client = _httpClientFactory.CreateClient();
            var request = new ProcessPaymentRequest
            {
                OrderId = orderId,
                CustomerId = customerId,
                Amount = amount
            };

            var content = new StringContent(
                JsonSerializer.Serialize(request),
                Encoding.UTF8,
                "application/json");

            var response = await client.PostAsync($"{PaymentServiceUrl}/api/payments", content);
            
            var responseContent = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<PaymentResponse>(responseContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            })!;
        }

        private async Task ConfirmOrderAsync(string orderId)
        {
            var client = _httpClientFactory.CreateClient();
            var response = await client.PutAsync($"{OrderServiceUrl}/api/orders/{orderId}/confirm", null);
            response.EnsureSuccessStatusCode();
        }

        private async Task CancelOrderAsync(string orderId, string reason)
        {
            var client = _httpClientFactory.CreateClient();
            var request = new OrderStatusUpdateRequest
            {
                OrderId = orderId,
                Reason = reason
            };

            var content = new StringContent(
                JsonSerializer.Serialize(request),
                Encoding.UTF8,
                "application/json");

            var response = await client.PutAsync($"{OrderServiceUrl}/api/orders/{orderId}/cancel", content);
            response.EnsureSuccessStatusCode();
        }

        private async Task RefundPaymentAsync(string paymentId)
        {
            var client = _httpClientFactory.CreateClient();
            var response = await client.PostAsync($"{PaymentServiceUrl}/api/payments/{paymentId}/refund", null);
            response.EnsureSuccessStatusCode();
        }

        public SagaTransaction? GetTransaction(string transactionId)
        {
            lock (_lock)
            {
                return _transactions.TryGetValue(transactionId, out var saga) ? saga : null;
            }
        }

        public List<SagaTransaction> GetAllTransactions()
        {
            lock (_lock)
            {
                return _transactions.Values.ToList();
            }
        }
    }
}
