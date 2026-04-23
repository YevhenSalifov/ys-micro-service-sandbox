using Microsoft.Azure.Cosmos;
using OrderService.Domain.Entities;
using OrderService.Domain.Repositories;

namespace OrderService.Infrastructure;

public class CosmosOrderRepository : IOrderRepository
{
    private readonly Container _container;

    public CosmosOrderRepository(CosmosClient client)
    {
        // В реальному житті ці назви йдуть в appsettings.json
        _container = client.GetContainer("OrderDb", "Orders");
    }

    public async Task AddAsync(Order order, CancellationToken ct = default)
    {
        try 
        {
            // Cosmos DB очікує, що об'єкт матиме поле "id" (string)
            await _container.CreateItemAsync(order, new PartitionKey(order.Id), cancellationToken: ct);
        }
        catch (CosmosException ex)
        {
            // Тут ми обробляємо конфлікти (наприклад, дублікати ID)
            throw;
        }
    }
}