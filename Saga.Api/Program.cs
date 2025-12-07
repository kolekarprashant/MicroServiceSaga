using Saga.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Configure Kestrel to use specific port
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenLocalhost(5000); // Saga API Gateway Port
});

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add HttpClient for inter-service communication
builder.Services.AddHttpClient();

// Register Saga Orchestrator
builder.Services.AddSingleton<SagaOrchestratorService>();

var app = builder.Build();

// Configure the HTTP request pipeline
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Saga API Gateway v1");
    c.RoutePrefix = "swagger";
});

app.MapControllers();

app.Logger.LogInformation("Saga API Gateway started on http://localhost:5000");
app.Logger.LogInformation("Swagger UI available at http://localhost:5000/swagger");

app.Run();
