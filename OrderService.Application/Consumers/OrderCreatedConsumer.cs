using MassTransit;
using OrderService.Domain.Contracts;
using Microsoft.Extensions.Logging;
using OrderService.Domain.Repositories;
using OrderService.Domain.Entities;

namespace OrderService.Application.Consumers;

public class OrderCreatedConsumer : IConsumer<OrderCreated>
{
    private readonly IOrderRepository _repository;
    private readonly ILogger<OrderCreatedConsumer> _logger;

    public OrderCreatedConsumer(IOrderRepository repository, ILogger<OrderCreatedConsumer> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<OrderCreated> context)
    {
        if (context.Message == null)
        {
            _logger.LogWarning("Отримано порожнє повідомлення OrderCreated. Пропускаємо обробку.");
            return;
        }

        if (string.IsNullOrEmpty(context.Message.CustomerEmail) || context.Message.Amount <= 0)
        {
            _logger.LogWarning("Отримано некоректне повідомлення OrderCreated: {@Message}. Пропускаємо обробку.", context.Message);
            return;
        }

        if (Guid.TryParse(context.Message.OrderId.ToString(), out var orderId) == false)
        {
            throw new ArgumentException($"Некоректний OrderId: {context.Message.OrderId}");
        }

        if (context.Message.Amount > 10000)
        {
            throw new InvalidOperationException("Занадто велика сума для автоматичної обробки!");
        }

        var order = new Order 
        { 
            Id = orderId.ToString(), // Це стане нашим Partition Key
            CustomerEmail = context.Message.CustomerEmail,
            TotalAmount = context.Message.Amount,
            CreatedAt = DateTime.UtcNow
        };

        await _repository.AddAsync(order, context.CancellationToken);
        
        _logger.LogInformation("✅ Замовлення {OrderId} успішно збережено в Cosmos DB", order.Id);
    }
}
