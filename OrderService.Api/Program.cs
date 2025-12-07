using OrderService.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Configure Kestrel to use specific port
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenLocalhost(5001); // Order Service Port
});

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register Order Service
builder.Services.AddSingleton<OrderManagementService>();

var app = builder.Build();

// Configure the HTTP request pipeline
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Order Service API v1");
    c.RoutePrefix = "swagger";
});

app.MapControllers();

app.Logger.LogInformation("Order Service started on http://localhost:5001");
app.Logger.LogInformation("Swagger UI available at http://localhost:5001/swagger");

app.Run();
