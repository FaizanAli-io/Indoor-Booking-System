using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Microsoft.Azure.Cosmos;

using System;

var builder = FunctionsApplication.CreateBuilder(args);

// Because your function uses HttpRequest + IActionResult:
builder.ConfigureFunctionsWebApplication();

// Register CosmosClient for DI
builder.Services.AddSingleton<CosmosClient>(sp =>
{
    var conn = Environment.GetEnvironmentVariable("CosmosDbConnectionString");

    if (string.IsNullOrWhiteSpace(conn))
        throw new InvalidOperationException("COSMOS_DB_CONNECTION is missing.");

    return new CosmosClient(conn);
});

// App Insights (your existing config)
builder.Services
    .AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights();

var app = builder.Build();
app.Run();
