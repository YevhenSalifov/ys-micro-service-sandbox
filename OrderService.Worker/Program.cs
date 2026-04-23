using MassTransit;
using Microsoft.Azure.Cosmos;
using OrderService.Application.Consumers;
using OrderService.Domain.Repositories;
using OrderService.Infrastructure;
using OrderService.Worker.DatabaseInit;

var builder = Host.CreateApplicationBuilder(args);

// Видалили AddHostedService<Worker> - він тут зайвий

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<OrderCreatedConsumer>();
    x.AddConsumer<OrderCreatedFaultConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        // Для Docker Compose хост має бути "rabbitmq"
        var hostName = builder.Configuration["MessageBroker:Host"] ?? "rabbitmq";
        
        cfg.Host(hostName, "/", h =>
        {
            h.Username("guest");
            h.Password("guest");
        });

        // Налаштування ретраїв (щоб сервіс не падав, якщо кролик ще спить)
        cfg.UseMessageRetry(r => r.Interval(5, TimeSpan.FromSeconds(5)));

        cfg.UseRawJsonDeserializer();
        
        cfg.UseScheduledRedelivery(r => r.Intervals(
            TimeSpan.FromMinutes(5), 
            TimeSpan.FromMinutes(15), 
            TimeSpan.FromMinutes(30)));

        cfg.ConfigureEndpoints(context);
    });
});

builder.Services.AddSingleton(sp => 
{
    // Стандартні налаштування для локального емулятора
    var endpoint = "https://localhost:8081";
    var key = "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==";
    
    return new CosmosClient(endpoint, key, new CosmosClientOptions
    {
        // Ігноруємо сертифікати емулятора
        HttpClientFactory = () => new HttpClient(new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (m, c, ch, e) => true
        }),
        ConnectionMode = ConnectionMode.Gateway,
        LimitToEndpoint = true
    });
});

builder.Services.AddScoped<IOrderRepository, CosmosOrderRepository>();

var host = builder.Build();

await DatabaseInit.EnsureCosmosDbResourcesCreated(host.Services);

await host.RunAsync();