using PaymentService.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Configure Kestrel to use specific port
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenLocalhost(5002); // Payment Service Port
});

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register Payment Service
builder.Services.AddSingleton<PaymentProcessingService>();

var app = builder.Build();

// Configure the HTTP request pipeline
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Payment Service API v1");
    c.RoutePrefix = "swagger";
});

app.MapControllers();

app.Logger.LogInformation("Payment Service started on http://localhost:5002");
app.Logger.LogInformation("Swagger UI available at http://localhost:5002/swagger");

app.Run();
