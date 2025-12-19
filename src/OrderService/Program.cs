using MassTransit;
using OrderService;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add MassTransit
builder.Services.AddMassTransit(cfg =>
{
    cfg.AddConsumer<OrderCompletedConsumer>();
    cfg.AddConsumer<OrderFailedConsumer>();

    cfg.UsingInMemory((ctx, cfgBus) =>
    {
        cfgBus.ConfigureEndpoints(ctx);
    });
});

// Add order tracking service
builder.Services.AddSingleton<OrderTrackingService>();

var app = builder.Build();

// Configure the HTTP request pipeline
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Order Service API v1");
    c.RoutePrefix = "swagger";
});

app.UseAuthorization();
app.MapControllers();

var port = Environment.GetEnvironmentVariable("PORT") ?? "5001";
var urls = $"http://0.0.0.0:{port}";

app.Logger.LogInformation("🛒 Order Service API starting on {Urls}", urls);
app.Logger.LogInformation("📖 Swagger UI available at {Urls}/swagger", urls);

app.Run(urls);
