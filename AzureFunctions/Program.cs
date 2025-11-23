using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Azure.Cosmos;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices((context, services) =>
    {
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();

        // Add Cosmos DB Client
        var cosmosEndpoint = "https://indoorbookingsystem.documents.azure.com:443/";
        var cosmosKey = context.Configuration["CosmosDB__AccountKey"];
        
        services.AddSingleton(s =>
        {
            return new CosmosClient(cosmosEndpoint, cosmosKey);
        });
    })
    .Build();

host.Run();
