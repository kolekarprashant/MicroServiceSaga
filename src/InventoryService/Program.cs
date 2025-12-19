using MassTransit;
using InventoryService;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add MassTransit
builder.Services.AddMassTransit(cfg =>
{
    cfg.AddConsumer<ReserveInventoryConsumer>();
    cfg.AddConsumer<ReleaseInventoryConsumer>();

    cfg.UsingInMemory((ctx, cfgBus) =>
    {
        cfgBus.ConfigureEndpoints(ctx);
    });
});

// Add inventory tracking service
builder.Services.AddSingleton<InventoryTrackingService>();

var app = builder.Build();

// Configure the HTTP request pipeline
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Inventory Service API v1");
    c.RoutePrefix = "swagger";
});

app.UseAuthorization();
app.MapControllers();

var port = Environment.GetEnvironmentVariable("PORT") ?? "5003";
var urls = $"http://0.0.0.0:{port}";

app.Logger.LogInformation("📦 Inventory Service API starting on {Urls}", urls);
app.Logger.LogInformation("📖 Swagger UI available at {Urls}/swagger", urls);

app.Run(urls);
app.Logger.LogInformation("📖 Swagger UI available at http://localhost:5003/swagger");

app.Run("http://localhost:5003");
