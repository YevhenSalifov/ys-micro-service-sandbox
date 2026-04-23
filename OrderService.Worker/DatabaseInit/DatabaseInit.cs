using Microsoft.Azure.Cosmos;
using OrderService.Domain.Entities;

namespace OrderService.Worker.DatabaseInit;

public class DatabaseInit
{
    public static async Task EnsureCosmosDbResourcesCreated(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var client = scope.ServiceProvider.GetRequiredService<CosmosClient>();

        var databaseResponse = await client.CreateDatabaseIfNotExistsAsync("OrderDb");
        var containerResponse = await databaseResponse.Database.CreateContainerIfNotExistsAsync(
            id: "Orders",
            partitionKeyPath: "/id"
        );

        var container = containerResponse.Container;

        // Запускаємо наповнення даними
        await SeedDataAsync(container);
    }

    private static async Task SeedDataAsync(Container container)
    {
        // 1. Перевіряємо, чи база вже порожня (щоб не дублювати дані)
        var query = new QueryDefinition("SELECT VALUE COUNT(1) FROM c");
        using var resultSet = container.GetItemQueryIterator<int>(query);
        var count = 0;
        if (resultSet.HasMoreResults)
        {
            var response = await resultSet.ReadNextAsync();
            count = response.First();
        }

        if (count > 0)
        {
            Console.WriteLine($"🚀 База вже містить {count} записів. Пропускаємо seeding.");
            return;
        }

        Console.WriteLine("🌱 Наповнюємо базу тестовими даними (20 записів)...");

        // 2. Геруємо та додаємо 20 замовлень паралельно
        var tasks = Enumerable.Range(1, 20).Select(i =>
        {
            var order = new Order
            {
                Id = Guid.NewGuid().ToString(),
                CustomerEmail = $"user{i}@example.com",
                TotalAmount = new Random().Next(10, 1000) + (decimal)new Random().NextDouble(),
                CreatedAt = DateTime.UtcNow.AddHours(-i) // Кожне наступне замовлення трохи старіше
            };

            return container.CreateItemAsync(order, new PartitionKey(order.Id));
        });

        await Task.WhenAll(tasks);
        Console.WriteLine("✅ Seeding завершено успішно!");
    }
}