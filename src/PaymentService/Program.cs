using MassTransit;
using PaymentService;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add MassTransit
builder.Services.AddMassTransit(cfg =>
{
    cfg.AddConsumer<ProcessPaymentConsumer>();

    cfg.UsingInMemory((ctx, cfgBus) =>
    {
        cfgBus.ConfigureEndpoints(ctx);
    });
});

// Add payment tracking service
builder.Services.AddSingleton<PaymentTrackingService>();

var app = builder.Build();

// Configure the HTTP request pipeline
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Payment Service API v1");
    c.RoutePrefix = "swagger";
});

app.UseAuthorization();
app.MapControllers();

var port = Environment.GetEnvironmentVariable("PORT") ?? "5002";
var urls = $"http://0.0.0.0:{port}";

app.Logger.LogInformation("💳 Payment Service API starting on {Urls}", urls);
app.Logger.LogInformation("📖 Swagger UI available at {Urls}/swagger", urls);

app.Run(urls);
app.Logger.LogInformation("📖 Swagger UI available at http://localhost:5002/swagger");

app.Run("http://localhost:5002");
