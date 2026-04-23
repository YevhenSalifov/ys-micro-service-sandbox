using OrderService.Domain.Entities;

namespace OrderService.Domain.Repositories;

// Переконайся, що тут стоїть public!
public interface IOrderRepository 
{
    Task AddAsync(Order order, CancellationToken ct = default);
}