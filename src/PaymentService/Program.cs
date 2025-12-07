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
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Payment Service API v1");
        c.RoutePrefix = "swagger";
    });
}

app.UseAuthorization();
app.MapControllers();

app.Logger.LogInformation("💳 Payment Service API started on http://localhost:5002");
app.Logger.LogInformation("📖 Swagger UI available at http://localhost:5002/swagger");

app.Run("http://localhost:5002");
