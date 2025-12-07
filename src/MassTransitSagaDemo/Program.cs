using MassTransit;
using MassTransitSagaDemo;

var builder = WebApplication.CreateBuilder(args);

// Enable detailed logging for debugging
builder.Logging.SetMinimumLevel(LogLevel.Debug);
builder.Logging.AddFilter("MassTransit", LogLevel.Debug);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add MassTransit with Saga
builder.Services.AddMassTransit(cfg =>
{
    cfg.AddSagaStateMachine<OrderStateMachine, OrderState>()
        .InMemoryRepository();

    cfg.UsingInMemory((ctx, cfgBus) =>
    {
        cfgBus.ConfigureEndpoints(ctx);
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Saga Orchestrator API v1");
        c.RoutePrefix = "swagger";
    });
}

app.UseAuthorization();
app.MapControllers();

app.Logger.LogInformation("🎯 Saga Orchestrator API started on http://localhost:5000");
app.Logger.LogInformation("📖 Swagger UI available at http://localhost:5000/swagger");

app.Run("http://localhost:5000");
